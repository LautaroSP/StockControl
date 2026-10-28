namespace StockControl.Dominio;

public static class ServicioCatalogoTienda
{
    public const int TamanoPagina = 24;
    public const int MaximoPorVez = 500;
    public const int LargoMaximoBusqueda = 80;
    public const int MaximoCategoriasFiltro = 50;

    public static bool PuedePublicar(Producto producto) => !producto.ProductoSector && !producto.EsGenerico;

    /// <summary>La regla de <see cref="PuedePublicar"/> traducible a SQL.</summary>
    public static IQueryable<Producto> Publicables(IQueryable<Producto> productos) =>
        productos.Where(p => !p.ProductoSector && !p.Codigo.ToUpper().StartsWith("GENERIC-"));

    public static void Publicar(Producto producto, bool publicado)
    {
        if (publicado && !PuedePublicar(producto))
            throw new ErrorNegocio($"«{producto.Nombre}» no se puede publicar: sector y genérico no tienen precio fijo.");
        producto.Publicado = publicado;
    }

    /// <summary>Publicados del local. Con categorías, solo los que tienen alguna de ellas como categoría efectiva.</summary>
    public static IQueryable<Producto> Publicados(
        IQueryable<Producto> productos, IQueryable<GrupoProductos> grupos, int idLocal, string? busqueda, IReadOnlyCollection<int>? categorias)
    {
        var consulta = Filtrar(productos, idLocal, busqueda);
        if (categorias is { Count: > 0 })
        {
            var ids = categorias.Take(MaximoCategoriasFiltro).Select(id => (int?)id).ToList();
            consulta = ConCategoria(consulta, grupos, idLocal).Where(x => ids.Contains(x.IdCategoria)).Select(x => x.Producto);
        }
        return consulta.OrderBy(p => p.Nombre).ThenBy(p => p.Id);
    }

    /// <summary>Categorías del local que son la efectiva de al menos un producto publicado.</summary>
    public static IQueryable<CategoriaTienda> CategoriasConPublicados(
        IQueryable<Producto> productos, IQueryable<GrupoProductos> grupos, IQueryable<Categoria> categorias, int idLocal)
    {
        var efectivas = ConCategoria(Filtrar(productos, idLocal, null), grupos, idLocal).Select(x => x.IdCategoria);
        return categorias
            .Where(c => c.IdLocal == idLocal && efectivas.Contains(c.Id))
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaTienda(c.Id, c.Nombre));
    }

    public static ProductoTienda AProductoTienda(
        Producto producto, GrupoProductos? grupo, IEnumerable<ImagenProducto>? imagenes = null, IReadOnlyDictionary<int, decimal>? reservado = null) =>
        new(producto.Id, producto.Codigo, producto.Nombre, producto.Precio, ServicioCategorias.Efectiva(producto, grupo),
            ServicioPedidosTienda.Disponible(producto, reservado ?? new Dictionary<int, decimal>()) > 0,
            (imagenes ?? []).Where(i => i.IdProducto == producto.Id).OrderBy(i => i.Orden).Select(ServicioImagenesTienda.ATienda).ToList());

    static IQueryable<Producto> Filtrar(IQueryable<Producto> productos, int idLocal, string? busqueda)
    {
        var consulta = Publicables(productos).Where(p => p.IdLocal == idLocal && p.Publicado);
        var texto = (busqueda ?? "").Trim().ToLowerInvariant();
        if (texto.Length > LargoMaximoBusqueda)
            texto = texto[..LargoMaximoBusqueda];
        if (texto.Length > 0)
            consulta = consulta.Where(p => p.Nombre.ToLower().Contains(texto) || p.Codigo.ToLower().Contains(texto));
        return consulta;
    }

    static IQueryable<ProductoConCategoria> ConCategoria(IQueryable<Producto> productos, IQueryable<GrupoProductos> grupos, int idLocal) =>
        productos.Select(p => new ProductoConCategoria
        {
            Producto = p,
            IdCategoria = p.IdCategoria ?? grupos
                .Where(g => g.IdLocal == idLocal && g.IdGrupoProducto == p.IdGrupoProducto)
                .Select(g => g.IdCategoria)
                .FirstOrDefault()
        });

    sealed class ProductoConCategoria
    {
        public Producto Producto { get; init; } = null!;
        public int? IdCategoria { get; init; }
    }
}
