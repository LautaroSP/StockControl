using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Pagos;

/// <summary>Procesa cada aviso del webhook pidiendo el dato a la API de Mercado Pago; no confía en el cuerpo del aviso.</summary>
public static class AvisosMercadoPago
{
    public static Task Procesar(string? tipo, string id, AppDbContext db, ClienteMercadoPago mp) => tipo switch
    {
        "payment" => PagoUnico(id, db, mp),
        "subscription_preapproval" => CambioSuscripcion(id, db, mp),
        "subscription_authorized_payment" => CobroDebito(id, db, mp),
        _ => Task.CompletedTask
    };

    private static async Task PagoUnico(string id, AppDbContext db, ClienteMercadoPago mp)
    {
        var pago = await mp.ObtenerPago(id);
        if (ClienteMercadoPago.IdAbono(pago.Referencia) is not { } idAbono) return;
        var abono = await db.Abonos.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == idAbono);
        if (abono == null) return;

        ServicioAbono.ValidarPagoMp(pago.Estado, pago.Moneda, pago.Importe);
        var local = await db.Locales.FirstAsync(l => l.IdLocal == abono.IdLocal);
        if (abono.Origen == Abono.OrigenMp)
            ServicioAbono.ValidarPagoManual(local);
        ServicioAbono.Acreditar(local, abono, pago.Fecha ?? DateTimeOffset.UtcNow, id);
        await db.SaveChangesAsync();
    }

    private static async Task CambioSuscripcion(string id, AppDbContext db, ClienteMercadoPago mp)
    {
        var suscripcion = await mp.ObtenerSuscripcion(id);
        var local = await db.Locales.FirstOrDefaultAsync(l => l.IdSuscripcionMp == suscripcion.Id);
        if (local == null) return;

        local.EstadoSuscripcion = ServicioAbono.EstadoSuscripcionMp(suscripcion.Estado);
        await db.SaveChangesAsync();
    }

    private static async Task CobroDebito(string id, AppDbContext db, ClienteMercadoPago mp)
    {
        var cobro = await mp.ObtenerCobroSuscripcion(id);
        if (cobro.EstadoPago != "approved" || cobro.IdPago == null) return;
        var local = await db.Locales.FirstOrDefaultAsync(l => l.IdSuscripcionMp == cobro.IdSuscripcion);
        if (local == null) return;
        if (await db.Abonos.IgnoreQueryFilters().AnyAsync(a => a.IdPagoMp == cobro.IdPago)) return;

        ServicioAbono.ValidarPagoMp(cobro.EstadoPago, cobro.Moneda, cobro.Importe);
        if (local.EstadoSuscripcion == ServicioAbono.DebitoPendiente)
            local.EstadoSuscripcion = ServicioAbono.DebitoActivo;
        var abono = ServicioAbono.NuevoAbono(local.IdLocal, Abono.OrigenDebito, DateTimeOffset.UtcNow);
        db.Abonos.Add(abono);
        ServicioAbono.Acreditar(local, abono, cobro.Fecha ?? DateTimeOffset.UtcNow, cobro.IdPago);
        await db.SaveChangesAsync();
    }
}
