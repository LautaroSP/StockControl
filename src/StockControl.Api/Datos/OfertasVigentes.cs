using Microsoft.EntityFrameworkCore;
using StockControl.Dominio;

namespace StockControl.Api.Datos;

public static class OfertasVigentes
{
    public static Task<List<Oferta>> DelDia(AppDbContext db, int idLocal, DateOnly dia) =>
        db.Ofertas.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(o => o.IdLocal == idLocal && o.FechaDesde <= dia && dia <= o.FechaHasta)
            .ToListAsync();

    /// <summary>Las del local marcadas «también en la tienda» y vigentes ese día. Nunca de otro local.</summary>
    public static IQueryable<Oferta> DeTienda(AppDbContext db, int idLocal, DateOnly dia) =>
        db.Ofertas.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(o => o.IdLocal == idLocal && o.EnTienda && o.FechaDesde <= dia && dia <= o.FechaHasta);

    public static Task<List<OfertaCombo>> CombosDelDia(AppDbContext db, int idLocal, DateOnly dia) =>
        db.OfertasCombo.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(c => c.Lineas)
            .Where(c => c.IdLocal == idLocal && c.FechaDesde <= dia && dia <= c.FechaHasta)
            .ToListAsync();
}
