using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

/// <summary>La deuda vencida cierra la tienda pública. La caja no entra en esta cuenta.</summary>
public static class ConsultaGestionTienda
{
    public static async Task<bool> Vencida(AppDbContext db, int idLocal, DateTimeOffset ahora)
    {
        var corte = ahora.AddDays(-ServicioGestionTienda.DiasParaPagar);
        return await db.GestionesTienda.IgnoreQueryFilters().AnyAsync(g =>
            g.IdLocal == idLocal && g.Estado == GestionTienda.Pendiente && g.VencePeriodo <= corte);
    }
}
