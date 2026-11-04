using System.Globalization;
using Microsoft.EntityFrameworkCore;
using StockControl.Api.Contratos;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

/// <summary>Admin: eximir locales, saldar la deuda aparte del mes y ver la cobrada, la pendiente y la saldada.</summary>
public static class EndpointsGestionTienda
{
    private const int TopeLocales = 500;

    public static void MapGestionTienda(this WebApplication app)
    {
        app.MapPost("/admin/locales/{idLocal:int}/gestion", Eximir).RequireAuthorization().RequireRateLimiting("api");
        app.MapPost("/admin/locales/{idLocal:int}/gestion/saldar", Saldar).RequireAuthorization().RequireRateLimiting("api");
        app.MapPost("/admin/locales/{idLocal:int}/gestion/anular-saldo", AnularSaldo).RequireAuthorization().RequireRateLimiting("api");
        app.MapGet("/admin/gestion-tienda", Cobrada).RequireAuthorization().RequireRateLimiting("api");
    }

    private static async Task<IResult> Eximir(int idLocal, GestionAdminPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config)
    {
        if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
        var local = await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == idLocal);
        if (local == null) return Results.NotFound();
        try
        {
            ServicioAdmin.ValidarClaveSecreta(pedido.Clave, config["Admin:Clave"]);
            local.ExentoGestion = pedido.Exento;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> Saldar(int idLocal, ClaveAdminPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config)
    {
        if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
        if (await db.Locales.AllAsync(l => l.IdLocal != idLocal)) return Results.NotFound();
        var deudas = await db.GestionesTienda.IgnoreQueryFilters()
            .Where(g => g.IdLocal == idLocal && g.Estado == GestionTienda.Pendiente && g.IdAbono == null)
            .ToListAsync();
        try
        {
            ServicioAdmin.ValidarClaveSecreta(pedido.Clave, config["Admin:Clave"]);
            var ahora = DateTimeOffset.UtcNow;
            var abono = ServicioGestionTienda.PrepararSaldoAdmin(idLocal, deudas, ahora);
            db.Abonos.Add(abono);
            await db.SaveChangesAsync();
            ServicioGestionTienda.MarcarSaldadas(deudas, abono.Id, ahora);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> AnularSaldo(int idLocal, ClaveAdminPedido pedido, AppDbContext db, SesionActual sesion, IConfiguration config)
    {
        if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
        var abono = await db.Abonos.IgnoreQueryFilters()
            .Where(a => a.IdLocal == idLocal && a.Estado == Abono.Aprobado && a.Origen == Abono.OrigenGestionAdmin)
            .OrderByDescending(a => a.FechaAcreditacion)
            .ThenByDescending(a => a.Id)
            .FirstOrDefaultAsync();
        if (abono == null) return Results.BadRequest("No hay una gestión saldada por el admin para anular.");
        var deudas = await db.GestionesTienda.IgnoreQueryFilters().Where(g => g.IdAbono == abono.Id).ToListAsync();
        try
        {
            ServicioAdmin.ValidarClaveSecreta(pedido.Clave, config["Admin:Clave"]);
            ServicioGestionTienda.AnularSaldoAdmin(abono, deudas);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    /// <summary>Mes de Argentina, formato aaaa-mm. Sin mes, el actual.</summary>
    private static async Task<IResult> Cobrada(string? mes, AppDbContext db, SesionActual sesion)
    {
        if (!Permisos.PuedeAdministrarPlataforma(sesion.Rol ?? "")) return Results.Forbid();
        var hoy = DiaArgentina.Hoy();
        var inicio = new DateOnly(hoy.Year, hoy.Month, 1);
        if (!string.IsNullOrWhiteSpace(mes)
            && !DateOnly.TryParseExact(mes + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out inicio))
            return Results.BadRequest("El mes va como aaaa-mm.");
        var desde = DiaArgentina.Rango(inicio).desde;
        var hasta = DiaArgentina.Rango(inicio.AddMonths(1)).desde;

        var pedidos = await db.PedidosTienda.IgnoreQueryFilters().AsNoTracking()
            .Where(p => p.MedioPago == ServicioPagoPedidoTienda.MercadoPago && p.EstadoPago == ServicioPagoPedidoTienda.Pagado
                && p.Pagado >= desde && p.Pagado < hasta)
            .Select(p => new { p.IdLocal, p.Gestion })
            .ToListAsync();
        var gestiones = await db.GestionesTienda.IgnoreQueryFilters().AsNoTracking()
            .Where(g => g.Estado != GestionTienda.Anulada && (
                (g.Nacida >= desde && g.Nacida < hasta) || (g.FechaSaldo >= desde && g.FechaSaldo < hasta)))
            .Select(g => new { g.IdLocal, g.Importe, g.Estado, g.Nacida, g.FechaSaldo })
            .ToListAsync();
        var conSaldo = await db.Abonos.IgnoreQueryFilters().AsNoTracking()
            .Where(a => a.Origen == Abono.OrigenGestionAdmin && a.Estado == Abono.Aprobado)
            .Select(a => a.IdLocal)
            .Distinct()
            .ToListAsync();
        var locales = await db.Locales.AsNoTracking().ToDictionaryAsync(l => l.IdLocal, l => l.Nombre);
        var ids = pedidos.Select(p => p.IdLocal).Concat(gestiones.Select(g => g.IdLocal)).Distinct();
        var items = ids.Select(id =>
        {
            var mp = pedidos.Where(p => p.IdLocal == id).ToList();
            var delLocal = gestiones.Where(g => g.IdLocal == id).ToList();
            var pendiente = delLocal.Where(g => g.Estado == GestionTienda.Pendiente && g.Nacida >= desde && g.Nacida < hasta).Sum(g => g.Importe);
            var saldada = delLocal.Where(g => g.Estado == GestionTienda.Saldada && g.FechaSaldo >= desde && g.FechaSaldo < hasta).Sum(g => g.Importe);
            return new
            {
                idLocal = id,
                nombre = locales.GetValueOrDefault(id, ""),
                pedidos = mp.Count,
                gestion = mp.Sum(p => p.Gestion),
                fueraDeMp = delLocal.Count(g => g.Nacida >= desde && g.Nacida < hasta),
                pendiente,
                saldada,
                puedeAnularSaldo = conSaldo.Contains(id)
            };
        })
        .Where(i => i.pedidos > 0 || i.fueraDeMp > 0 || i.pendiente > 0 || i.saldada > 0)
        .OrderByDescending(i => i.gestion + i.pendiente + i.saldada)
        .Take(TopeLocales)
        .ToList();
        return Results.Ok(new
        {
            mes = inicio.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            importe = ServicioGestionTienda.ImportePorPedido,
            total = items.Sum(i => i.gestion),
            totalPendiente = items.Sum(i => i.pendiente),
            totalSaldada = items.Sum(i => i.saldada),
            items
        });
    }
}
