using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

public static class LocalesTienda
{
    /// <summary>Busca por alias sin filtro de local. Un alias imposible ni llega a la base.</summary>
    public static async Task<Local?> PorAlias(AppDbContext db, string alias)
    {
        var valor = alias.Trim().ToLowerInvariant();
        if (valor.Length < ServicioTienda.LargoMinimoAlias || valor.Length > ServicioTienda.LargoMaximoAlias)
            return null;
        return await db.Locales.AsNoTracking().FirstOrDefaultAsync(l => l.AliasTienda == valor);
    }
}
