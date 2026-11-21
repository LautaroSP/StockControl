namespace StockControl.Dominio;

public static class ServicioCatalogoTienda
{
    /// <summary>Filas que pide el scroll infinito del catálogo.</summary>
    public const int FilasPorCarga = 4;
    public const int TamanoPagina = 24;
    public const int TamanoMinimo = 8;
    public const int TamanoMaximo = 48;
    public const int MaximoPorVez = 500;
    public const int LargoMaximoBusqueda = 80;
    public const int MaximoCategoriasFiltro = 50;

    /// <summary>El front manda columnas × 4 filas. Sin valor o fuera de rango: 24.</summary>
    public static int NormalizarTamano(int tamano) =>
        tamano <= 0 ? TamanoPagina : Math.Clamp(tamano, TamanoMinimo, TamanoMaximo);

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
        IQueryable<Producto> productos, IQueryable<GrupoProductos> grupos, int idLocal, string? busqueda, IReadOnlyCollection<int>? categorias,
        string? orden = null, string? sentido = null, IQueryable<Oferta>? ofertasTienda = null)
    {
        var consulta = Filtrar(productos, idLocal, busqueda);
        if (categorias is { Count: > 0 })
        {
            var ids = categorias.Take(MaximoCategoriasFiltro).Select(id => (int?)id).ToList();
            consulta = ConCategoria(consulta, grupos, idLocal).Where(x => ids.Contains(x.IdCategoria)).Select(x => x.Producto);
        }
        return Ordenar(consulta, ofertasTienda, orden, sentido);
    }

    /// <summary>Categorías con publicados propios o en hijos, con cantidad acumulada en el padre.</summary>
    public static List<CategoriaTienda> CategoriasConPublicados(
        IEnumerable<Categoria> categorias, IEnumerable<Producto> publicados, IEnumerable<GrupoProductos> grupos)
    {
        var propios = ServicioCategorias.ContarProductos(publicados, grupos);
        var conPadres = ServicioCategorias.ContarConPadres(propios, categorias);
        return categorias
            .Where(c => conPadres.GetValueOrDefault(c.Id) > 0)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaTienda(c.Id, c.Nombre, c.IdPadre, conPadres.GetValueOrDefault(c.Id)))
            .ToList();
    }

    public const string OrdenNombre = "nombre";
    public const string OrdenPrecio = "precio";
    public const int LargoMaximoDescripcion = 500;

    /// <summary>Lista blanca: cualquier otro valor ordena por nombre.</summary>
    public static string NormalizarOrden(string? orden) =>
        (orden ?? "").Trim().ToLowerInvariant() == OrdenPrecio ? OrdenPrecio : OrdenNombre;

    /// <summary>Vacío = sin descripción. Texto plano de hasta 500 caracteres.</summary>
    public static string? NormalizarDescripcion(string? descripcion)
    {
        var texto = (descripcion ?? "").Trim();
        if (texto.Length > LargoMaximoDescripcion)
            throw new ErrorNegocio($"La descripción admite hasta {LargoMaximoDescripcion} caracteres.");
        return texto.Length == 0 ? null : texto;
    }

    /// <summary>Por precio, el de una unidad con la oferta de tienda vigente (solo el descuento lo baja). Desempata por nombre.</summary>
    public static IQueryable<Producto> Ordenar(IQueryable<Producto> productos, IQueryable<Oferta>? ofertasTienda, string? orden, string? sentido)
    {
        var descendente = (sentido ?? "").Trim().ToLowerInvariant() == "desc";
        if (NormalizarOrden(orden) == OrdenNombre)
            return descendente
                ? productos.OrderByDescending(p => p.Nombre).ThenBy(p => p.Id)
                : productos.OrderBy(p => p.Nombre).ThenBy(p => p.Id);
        if (ofertasTienda == null)
            return (descendente ? productos.OrderByDescending(p => p.Precio) : productos.OrderBy(p => p.Precio))
                .ThenBy(p => p.Nombre).ThenBy(p => p.Id);
        var conPrecio = from p in productos
                        join o in ofertasTienda on p.Id equals o.IdProducto into g
                        from o in g.DefaultIfEmpty()
                        select new { p, precio = o != null && o.Tipo == TipoOferta.Descuento ? p.Precio * (100 - o.Porcentaje) / 100m : p.Precio };
        var ordenado = descendente ? conPrecio.OrderByDescending(x => x.precio) : conPrecio.OrderBy(x => x.precio);
        return ordenado.ThenBy(x => x.p.Nombre).ThenBy(x => x.p.Id).Select(x => x.p);
    }

    public static ProductoTienda AProductoTienda(
        Producto producto, GrupoProductos? grupo, IEnumerable<ImagenProducto>? imagenes = null, IReadOnlyDictionary<int, decimal>? reservado = null,
        IEnumerable<Oferta>? ofertasTienda = null, DateOnly hoy = default)
    {
        var oferta = ServicioOfertas.AplicableEnTienda(ofertasTienda ?? [], producto.Id, hoy);
        var precio = ServicioOfertas.PrecioUnitarioConOferta(oferta, producto.Precio, 1);
        return new(producto.Id, producto.Codigo, producto.Nombre, precio, ServicioCategorias.Efectiva(producto, grupo),
            ServicioPedidosTienda.Disponible(producto, reservado ?? new Dictionary<int, decimal>()) > 0,
            (imagenes ?? []).Where(i => i.IdProducto == producto.Id).OrderBy(i => i.Orden).Select(ServicioImagenesTienda.ATienda).ToList(),
            producto.Descripcion ?? "", producto.Precio, ServicioOfertas.Descripcion(oferta), producto.Precio - precio);
    }

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
