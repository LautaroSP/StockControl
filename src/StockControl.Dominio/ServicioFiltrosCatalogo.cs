namespace StockControl.Dominio;

public static class ServicioFiltrosCatalogo
{
    public static IEnumerable<Producto> Filtrar(
        IEnumerable<Producto> productos,
        string? tipo,
        int? idGrupo,
        bool sinGrupo,
        bool stockBajo,
        decimal umbral)
    {
        var consulta = productos;
        if (string.Equals(tipo, "sector", StringComparison.OrdinalIgnoreCase))
            consulta = consulta.Where(p => p.ProductoSector);
        else if (string.Equals(tipo, "comun", StringComparison.OrdinalIgnoreCase))
            consulta = consulta.Where(p => !p.ProductoSector);
        if (idGrupo is > 0)
            consulta = consulta.Where(p => p.IdGrupoProducto == idGrupo.Value);
        else if (sinGrupo)
            consulta = consulta.Where(p => p.IdGrupoProducto == 0);
        if (stockBajo)
            consulta = consulta.Where(p => !p.ProductoSector && p.Cantidad <= umbral);
        return consulta;
    }
}
