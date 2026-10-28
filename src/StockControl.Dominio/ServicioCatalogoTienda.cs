namespace StockControl.Dominio;

public static class ServicioCatalogoTienda
{
    public const int TamanoPagina = 24;
    public const int MaximoPorVez = 500;
    public const int LargoMaximoBusqueda = 80;

    public static bool PuedePublicar(Producto producto) => !producto.ProductoSector && !producto.EsGenerico;

    public static void Publicar(Producto producto, bool publicado)
    {
        if (publicado && !PuedePublicar(producto))
            throw new ErrorNegocio($"«{producto.Nombre}» no se puede publicar: sector y genérico no tienen precio fijo.");
        producto.Publicado = publicado;
    }

    public static IQueryable<Producto> Publicados(IQueryable<Producto> productos, int idLocal, string? busqueda, int? idGrupo)
    {
        var consulta = productos.Where(p =>
            p.IdLocal == idLocal
            && p.Publicado
            && !p.ProductoSector
            && !p.Codigo.ToUpper().StartsWith("GENERIC-"));
        if (idGrupo is > 0)
            consulta = consulta.Where(p => p.IdGrupoProducto == idGrupo.Value);
        var texto = (busqueda ?? "").Trim().ToLowerInvariant();
        if (texto.Length > LargoMaximoBusqueda)
            texto = texto[..LargoMaximoBusqueda];
        if (texto.Length > 0)
            consulta = consulta.Where(p => p.Nombre.ToLower().Contains(texto) || p.Codigo.ToLower().Contains(texto));
        return consulta.OrderBy(p => p.Nombre).ThenBy(p => p.Id);
    }

    public static ProductoTienda AProductoTienda(Producto producto, IEnumerable<ImagenProducto>? imagenes = null) =>
        new(producto.Id, producto.Codigo, producto.Nombre, producto.Precio, producto.IdGrupoProducto, producto.Cantidad > 0,
            (imagenes ?? []).Where(i => i.IdProducto == producto.Id).OrderBy(i => i.Orden).Select(ServicioImagenesTienda.ATienda).ToList());
}
