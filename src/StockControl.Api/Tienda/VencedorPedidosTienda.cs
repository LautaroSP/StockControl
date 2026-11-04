using Microsoft.EntityFrameworkCore;
using StockControl.Api.Avisos;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

/// <summary>
/// Cada minuto cancela los pedidos que esperan el pago fuera de plazo y libera su stock.
/// La transferencia vencida se avisa: el local ya tenía el pedido.
/// </summary>
public class VencedorPedidosTienda(IServiceScopeFactory scopes, ColaPush cola, ILogger<VencedorPedidosTienda> log) : BackgroundService
{
    private static readonly TimeSpan Cada = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken cancelar)
    {
        using var reloj = new PeriodicTimer(Cada);
        while (await reloj.WaitForNextTickAsync(cancelar))
        {
            try
            {
                await Vencer(cancelar);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.LogWarning(ex, "No se pudieron vencer los pedidos de la tienda sin pagar");
            }
        }
    }

    private async Task Vencer(CancellationToken cancelar)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        scope.ServiceProvider.GetRequiredService<AlcanceDatos>().AbrirTodo();
        var limite = DateTimeOffset.UtcNow;
        var locales = await Pendientes(db, limite).Select(p => p.IdLocal).Distinct().ToListAsync(cancelar);
        foreach (var idLocal in locales)
        {
            await using var tx = await db.Database.BeginTransactionAsync(cancelar);
            await ReservasTienda.Bloquear(db, idLocal);
            var ahora = DateTimeOffset.UtcNow;
            var pedidos = await Pendientes(db, limite).Where(p => p.IdLocal == idLocal).ToListAsync(cancelar);
            var avisos = new List<MensajePush>();
            foreach (var pedido in pedidos.Where(p => ServicioPagoPedidoTienda.Vencido(p, ahora)))
            {
                ServicioPagoPedidoTienda.Vencer(pedido, ahora);
                if (pedido.MedioPago == ServicioPagoPedidoTienda.Transferencia)
                    avisos.Add(await AvisosPedidos.Agregar(db, pedido, ServicioAvisos.PagoVencido));
            }
            await db.SaveChangesAsync(cancelar);
            await tx.CommitAsync(cancelar);
            avisos.ForEach(cola.Encolar);
            db.ChangeTracker.Clear();
        }
    }

    private static IQueryable<PedidoTienda> Pendientes(AppDbContext db, DateTimeOffset limite) =>
        db.PedidosTienda.IgnoreQueryFilters().Where(p =>
            p.EstadoPago == ServicioPagoPedidoTienda.Pendiente
            && p.Estado == ServicioPedidosTienda.Nuevo
            && p.VencePago != null
            && p.VencePago <= limite);
}
