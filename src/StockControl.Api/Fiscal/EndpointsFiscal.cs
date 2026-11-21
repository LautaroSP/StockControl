using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Fiscal;

/// <summary>Factura electrónica ARCA: configuración del local, facturar una caja cerrada, estado y reintento. Dueño y socio.</summary>
public static class EndpointsFiscal
{
    private const int TopePagina = 100;

    public static void MapFiscal(this WebApplication app)
    {
        var fiscal = app.MapGroup("/fiscal").RequireAuthorization().RequireRateLimiting("api");
        fiscal.MapGet("/configuracion", LeerConfiguracion);
        fiscal.MapPut("/configuracion", GuardarConfiguracion);
        fiscal.MapGet("/comprobantes", Comprobantes);
        fiscal.MapPost("/comprobantes/{id:int}/reintentar", Reintentar);
        app.MapGet("/cajas/{idCierre:int}/fiscal", EstadoCaja).RequireAuthorization().RequireRateLimiting("api");
        app.MapPost("/cajas/{idCierre:int}/facturar", Facturar).RequireAuthorization().RequireRateLimiting("api");
    }

    private static async Task<IResult> LeerConfiguracion(AppDbContext db, SesionActual sesion, OpcionesArca opciones)
    {
        if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
        if (!Permisos.PuedeFacturar(sesion.Rol ?? "")) return Results.Forbid();
        var c = await db.ConfiguracionesFiscales.AsNoTracking().FirstOrDefaultAsync();
        return Results.Ok(new
        {
            cuit = c?.Cuit ?? "",
            puntoVenta = c?.PuntoVenta ?? 0,
            activa = c?.Activa ?? false,
            facturaGlobalPorDefecto = c?.FacturaGlobalPorDefecto ?? false,
            arcaDisponible = opciones.Disponible,
            ambiente = opciones.Ambiente,
            tope = opciones.TopeConsumidorFinal
        });
    }

    private static async Task<IResult> GuardarConfiguracion(ConfiguracionFiscalPedido pedido, AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
        if (!Permisos.PuedeFacturar(sesion.Rol ?? "")) return Results.Forbid();
        try
        {
            var c = await db.ConfiguracionesFiscales.FirstOrDefaultAsync();
            if (c == null)
            {
                c = new ConfiguracionFiscal { IdLocal = sesion.IdLocal.Value };
                db.ConfiguracionesFiscales.Add(c);
            }
            ConfiguracionFiscal.Guardar(c, pedido.Cuit, pedido.PuntoVenta, pedido.Activa, pedido.FacturaGlobalPorDefecto);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> Facturar(
        int idCierre, FacturarCajaPedido pedido, AppDbContext db, SesionActual sesion,
        IClienteArca? cliente, OpcionesArca opciones, CancellationToken ct)
    {
        if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
        if (!Permisos.PuedeFacturar(sesion.Rol ?? "")) return Results.Forbid();
        var modo = string.Equals(pedido.Modo, "global", StringComparison.OrdinalIgnoreCase)
            ? ModoFacturacion.Global
            : ModoFacturacion.PorVenta;
        try
        {
            var r = await OperacionesFiscales.FacturarCaja(db, sesion, cliente, opciones, idCierre, modo, pedido.IdsVentas, ct);
            return Results.Ok(new
            {
                comprobantes = r.Comprobantes.Select(Resumen),
                omitidas = r.Omitidas.Select(o => new { o.IdInformeVenta, o.Motivo })
            });
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (DbUpdateException)
        {
            return Results.Conflict("Esas ventas ya fueron facturadas. Actualizá la pantalla.");
        }
    }

    private static async Task<IResult> EstadoCaja(int idCierre, AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
        if (!Permisos.PuedeFacturar(sesion.Rol ?? "")) return Results.Forbid();
        var ventas = await db.InformeVenta.AsNoTracking().Where(v => v.IdCierre == idCierre)
            .Select(v => new { v.IdInformeVenta, v.Total }).Take(TopePagina * 10).ToListAsync();
        var ids = ventas.Select(v => v.IdInformeVenta).ToList();
        var facturadas = await db.ComprobantesFiscalesVentas.AsNoTracking()
            .Where(f => ids.Contains(f.IdInformeVenta)).Select(f => f.IdInformeVenta).ToListAsync();
        var comprobantes = await db.ComprobantesFiscales.AsNoTracking().Where(c => c.IdCierre == idCierre)
            .OrderBy(c => c.Id).Take(TopePagina).ToListAsync();
        return Results.Ok(new
        {
            ventas = ventas.Count,
            facturadas = facturadas.Count,
            sinFacturar = ventas.Count - facturadas.Count,
            totalSinFacturar = ventas.Where(v => !facturadas.Contains(v.IdInformeVenta)).Sum(v => v.Total),
            idsFacturadas = facturadas,
            comprobantes = comprobantes.Select(Resumen)
        });
    }

    private static async Task<IResult> Comprobantes(AppDbContext db, SesionActual sesion, int pagina = 1, int tamano = 50)
    {
        if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
        if (!Permisos.PuedeFacturar(sesion.Rol ?? "")) return Results.Forbid();
        tamano = Math.Clamp(tamano, 1, TopePagina);
        pagina = Math.Max(1, pagina);
        var total = await db.ComprobantesFiscales.CountAsync();
        var items = await db.ComprobantesFiscales.AsNoTracking().OrderByDescending(c => c.Id)
            .Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
        return Results.Ok(new { total, pagina, tamano, items = items.Select(Resumen) });
    }

    private static async Task<IResult> Reintentar(
        int id, AppDbContext db, SesionActual sesion, IClienteArca? cliente, CancellationToken ct)
    {
        if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
        if (!Permisos.PuedeFacturar(sesion.Rol ?? "")) return Results.Forbid();
        try
        {
            return Results.Ok(Resumen(await OperacionesFiscales.Reintentar(db, sesion, cliente, id, ct)));
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static object Resumen(ComprobanteFiscal c) => new
    {
        c.Id, c.Tipo, c.Alcance, c.Estado, c.PuntoVenta, c.Numero, c.Importe,
        c.Cae, c.VencimientoCae, c.Fecha, c.IdCierre, c.NroCaja, c.Observacion,
        cantidadVentas = c.Ventas.Count
    };
}
