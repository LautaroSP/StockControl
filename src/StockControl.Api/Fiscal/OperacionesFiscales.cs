using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Fiscal;

/// <summary>Une el dominio fiscal con la base. Serializa la emisión por local (misma numeración) dentro de este proceso.</summary>
public static class OperacionesFiscales
{
    private static readonly ConcurrentDictionary<int, SemaphoreSlim> Candados = new();

    public static async Task<ResultadoFacturacion> FacturarCaja(
        AppDbContext db, SesionActual sesion, IClienteArca? cliente, OpcionesArca opciones,
        int idCierre, ModoFacturacion modo, IReadOnlyCollection<int>? idsVentas, CancellationToken ct)
    {
        if (cliente == null)
            throw new ErrorNegocio("ARCA no está configurado en este servidor todavía.");
        var idLocal = sesion.IdLocal ?? throw new ErrorNegocio("Elegí un local.");
        var candado = Candados.GetOrAdd(idLocal, _ => new SemaphoreSlim(1, 1));
        await candado.WaitAsync(ct);
        try
        {
            var config = await db.ConfiguracionesFiscales.FirstOrDefaultAsync(ct);
            var ventas = await VentasDeLaCaja(db, idCierre, idsVentas, ct);
            if (ventas.Count == 0)
                throw new ErrorNegocio("No hay ventas en esa caja.");
            var ids = ventas.Select(v => v.IdInformeVenta).ToList();
            var ya = (await db.ComprobantesFiscalesVentas.Where(f => ids.Contains(f.IdInformeVenta))
                .Select(f => f.IdInformeVenta).ToListAsync(ct)).ToHashSet();
            var resultado = await ServicioFacturacion.FacturarCaja(
                cliente, config, sesion.Rol ?? "", idCierre, ventas, ya, modo,
                opciones.TopeConsumidorFinal, DateTimeOffset.UtcNow, ct);
            db.ComprobantesFiscales.AddRange(resultado.Comprobantes);
            await db.SaveChangesAsync(ct);
            return resultado;
        }
        finally
        {
            candado.Release();
        }
    }

    public static async Task<ComprobanteFiscal> Reintentar(
        AppDbContext db, SesionActual sesion, IClienteArca? cliente, int idComprobante, CancellationToken ct)
    {
        if (!Permisos.PuedeFacturar(sesion.Rol ?? ""))
            throw new ErrorNegocio("Solo el dueño o un socio pueden facturar.");
        if (cliente == null)
            throw new ErrorNegocio("ARCA no está configurado en este servidor todavía.");
        var idLocal = sesion.IdLocal ?? throw new ErrorNegocio("Elegí un local.");
        var candado = Candados.GetOrAdd(idLocal, _ => new SemaphoreSlim(1, 1));
        await candado.WaitAsync(ct);
        try
        {
            var comprobante = await db.ComprobantesFiscales.Include(c => c.Ventas).FirstOrDefaultAsync(c => c.Id == idComprobante, ct)
                ?? throw new ErrorNegocio("No existe el comprobante.");
            var config = await db.ConfiguracionesFiscales.FirstOrDefaultAsync(ct)
                ?? throw new ErrorNegocio("Configurá el CUIT y el punto de venta.");
            var origen = comprobante.IdComprobanteOrigen is int idOrigen
                ? await db.ComprobantesFiscales.FirstOrDefaultAsync(c => c.Id == idOrigen, ct)
                : null;
            try
            {
                await EmisorComprobante.Reintentar(cliente, config, comprobante, origen, ct);
            }
            catch (ExcepcionArca ex)
            {
                throw new ErrorNegocio($"No se pudo consultar ARCA: {ex.Message}");
            }
            if (comprobante.Estado == EstadoComprobanteFiscal.Rechazado)
                comprobante.Ventas.Clear();
            await db.SaveChangesAsync(ct);
            return comprobante;
        }
        finally
        {
            candado.Release();
        }
    }

    /// <summary>Factura vigente de la venta, si la tiene (y la fila que la vincula).</summary>
    public static async Task<(ComprobanteFiscal Factura, ComprobanteFiscalVenta Fila)?> FacturaDeVenta(
        AppDbContext db, int idInformeVenta, CancellationToken ct = default)
    {
        var fila = await db.ComprobantesFiscalesVentas.FirstOrDefaultAsync(f => f.IdInformeVenta == idInformeVenta, ct);
        if (fila == null) return null;
        var factura = await db.ComprobantesFiscales.FirstAsync(c => c.Id == fila.IdComprobanteFiscal, ct);
        return (factura, fila);
    }

    /// <summary>Al anular una venta facturada: emite la nota de crédito. Sin ARCA disponible queda Pendiente.</summary>
    public static async Task EmitirNotaCredito(
        AppDbContext db, IClienteArca? cliente, ComprobanteFiscal factura, ComprobanteFiscalVenta fila, CancellationToken ct)
    {
        var config = await db.ConfiguracionesFiscales.FirstOrDefaultAsync(ct)
            ?? throw new ErrorNegocio("Configurá el CUIT y el punto de venta antes de anular una venta facturada.");
        var candado = Candados.GetOrAdd(config.IdLocal, _ => new SemaphoreSlim(1, 1));
        await candado.WaitAsync(ct);
        try
        {
            var nota = await ServicioNotaCredito.Emitir(cliente, config, factura, fila, DateTimeOffset.UtcNow, ct);
            db.ComprobantesFiscales.Add(nota);
        }
        finally
        {
            candado.Release();
        }
    }

    private static async Task<List<InformeVenta>> VentasDeLaCaja(
        AppDbContext db, int idCierre, IReadOnlyCollection<int>? idsVentas, CancellationToken ct)
    {
        var consulta = db.InformeVenta.Where(v => v.IdCierre == idCierre);
        if (idsVentas is { Count: > 0 })
            consulta = consulta.Where(v => idsVentas.Contains(v.IdInformeVenta));
        return await consulta.OrderBy(v => v.IdInformeVenta).ToListAsync(ct);
    }
}
