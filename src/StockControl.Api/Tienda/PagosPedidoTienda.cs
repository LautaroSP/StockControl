using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using StockControl.Api.Avisos;
using StockControl.Api.Datos;
using StockControl.Api.Pagos;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

/// <summary>Checkout Pro de la tienda con la cuenta MP del local. No usa el token del abono.</summary>
public static class PagosPedidoTienda
{
    public static Task<bool> AceptaMercadoPago(AppDbContext db, int idLocal) =>
        db.CuentasMercadoPago.IgnoreQueryFilters().AnyAsync(c => c.IdLocal == idLocal);

    /// <summary>Si MP no arma la preferencia, el pedido se cancela y libera el stock.</summary>
    public static async Task IniciarPago(
        AppDbContext db, ClienteMercadoPagoLocal mp, IDataProtector protector, IConfiguration config,
        Local local, PedidoTienda pedido, string codigo)
    {
        try
        {
            var cuenta = await db.CuentasMercadoPago.IgnoreQueryFilters().FirstAsync(c => c.IdLocal == local.IdLocal);
            var token = await CobrosMercadoPagoApp.TokenVigente(db, mp, protector, config, cuenta);
            var vuelta = $"{Url(config, "App:UrlWeb")}/tienda/{Uri.EscapeDataString(local.AliasTienda ?? "")}/pedido/{Uri.EscapeDataString(codigo)}";
            var aviso = $"{Url(config, "App:UrlApi")}/mercadopago/webhook?tienda={local.IdLocal}";
            pedido.UrlPagoMp = await mp.CrearPreferenciaTienda(
                token, pedido, local.Nombre, vuelta, aviso, pedido.VencePago!.Value);
            await db.SaveChangesAsync();
        }
        catch
        {
            ServicioPedidosTienda.CancelarDelLocal(pedido, DateTimeOffset.UtcNow);
            await db.SaveChangesAsync();
            throw;
        }
    }

    /// <summary>
    /// Pide el pago a MP con el token del local y lo valida contra el pedido. Un pago que llega tarde o repetido se devuelve.
    /// </summary>
    public static async Task AplicarAviso(
        AppDbContext db, SesionActual sesion, ClienteMercadoPagoLocal mp, IDataProtector protector, IConfiguration config,
        ColaPush cola, ILogger log, int idLocal, string idPago)
    {
        var cuenta = await db.CuentasMercadoPago.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.IdLocal == idLocal);
        if (cuenta == null) return;
        var token = await CobrosMercadoPagoApp.TokenVigente(db, mp, protector, config, cuenta);
        var pago = await mp.ObtenerPago(token, idPago);
        if (pago.Id != idPago || ServicioPagoPedidoTienda.IdDeReferencia(pago.Referencia) is not int idPedido) return;

        sesion.IdLocal = idLocal;
        await using var tx = await db.Database.BeginTransactionAsync();
        await ReservasTienda.Bloquear(db, idLocal);
        var pedido = await db.PedidosTienda.Include(p => p.Lineas).FirstOrDefaultAsync(p => p.Id == idPedido);
        if (pedido == null) return;

        var resultado = ServicioPagoPedidoTienda.Aplicar(pedido, pago, cuenta.IdUsuarioMp, DateTimeOffset.UtcNow);
        if (resultado == ResultadoPagoTienda.Acreditado)
        {
            var push = await AvisosPedidos.Agregar(db, pedido, ServicioAvisos.PedidoNuevo);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            cola.Encolar(push);
        }
        else if (resultado == ResultadoPagoTienda.Devolver)
        {
            await mp.ReembolsarPago(token, pago.Id);
            ServicioPagoPedidoTienda.RegistrarDevolucion(pedido, pago.Id);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            log.LogWarning("Pago {IdPago} del pedido {IdPedido} devuelto: el pedido estaba cancelado o ya pagado", pago.Id, pedido.Id);
        }
    }

    /// <summary>Fuera de la transacción del pedido: si se renueva, el token nuevo no se pierde con un rollback.</summary>
    public static async Task<string> Token(
        AppDbContext db, ClienteMercadoPagoLocal mp, IDataProtector protector, IConfiguration config, int idLocal)
    {
        var cuenta = await db.CuentasMercadoPago.FirstOrDefaultAsync(c => c.IdLocal == idLocal)
            ?? throw new ErrorNegocio("El local ya no tiene Mercado Pago conectado. Devolvé el pago desde la app de Mercado Pago.");
        return await CobrosMercadoPagoApp.TokenVigente(db, mp, protector, config, cuenta);
    }

    /// <summary>Primero devuelve en MP; si MP falla, el pedido sigue pagado.</summary>
    public static async Task CancelarConReembolso(AppDbContext db, ClienteMercadoPagoLocal mp, string token, PedidoTienda pedido)
    {
        ServicioPagoPedidoTienda.CancelarConReembolso(pedido, DateTimeOffset.UtcNow);
        await mp.ReembolsarPago(token, pedido.IdPagoMp!);
        await db.SaveChangesAsync();
    }

    private static string Url(IConfiguration config, string clave) =>
        CobrosMercadoPagoApp.Exigir(config, clave).TrimEnd('/');
}
