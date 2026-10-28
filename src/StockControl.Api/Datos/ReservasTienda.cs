using Microsoft.EntityFrameworkCore;
using StockControl.Dominio;

namespace StockControl.Api.Datos;

public static class ReservasTienda
{
    public static async Task<Dictionary<int, decimal>> DelLocal(AppDbContext db, int idLocal) =>
        ServicioPedidosTienda.Reservado(await db.PedidosTienda.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(p => p.Lineas)
            .Where(p => p.IdLocal == idLocal && p.Estado == ServicioPedidosTienda.Nuevo)
            .ToListAsync());

    /// <summary>Serializa las reservas del local hasta el fin de la transacción.</summary>
    public static Task Bloquear(AppDbContext db, int idLocal) =>
        db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(37, {idLocal})");
}
