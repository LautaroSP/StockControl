using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCatalogoTiendaTests
{
    private static Producto Producto(int id, int local = 1, bool publicado = true, bool sector = false, string? codigo = null, string? nombre = null, int grupo = 0, decimal cantidad = 10, int? categoria = null) => new()
    {
        IdCategoria = categoria,
        Id = id,
        IdLocal = local,
        Codigo = codigo ?? $"779{id}",
        Nombre = nombre ?? $"Producto {id}",
        Precio = 1000,
        Costo = 600,
        Cantidad = cantidad,
        ProductoSector = sector,
        IdGrupoProducto = grupo,
        Publicado = publicado
    };

    private static GrupoProductos Grupo(int id, int? categoria, int local = 1) =>
        new() { IdGrupoProducto = id, IdLocal = local, NombreGrupo = $"Grupo {id}", IdCategoria = categoria };

    private static Categoria Categoria(int id, string nombre, int local = 1) => new() { Id = id, IdLocal = local, Nombre = nombre };

    private static List<int> Ids(IEnumerable<Producto> productos, int idLocal = 1, string? busqueda = null, int[]? categorias = null, IEnumerable<GrupoProductos>? grupos = null) =>
        ServicioCatalogoTienda.Publicados(productos.AsQueryable(), (grupos ?? []).AsQueryable(), idLocal, busqueda, categorias)
            .Select(p => p.Id).ToList();

    private static List<string> Categorias(IEnumerable<Producto> productos, IEnumerable<Categoria> categorias, IEnumerable<GrupoProductos>? grupos = null) =>
        ServicioCatalogoTienda.CategoriasConPublicados(productos.AsQueryable(), (grupos ?? []).AsQueryable(), categorias.AsQueryable(), 1)
            .Select(c => c.Nombre).ToList();

    [Fact]
    public void Solo_salen_productos_publicados()
    {
        var productos = new[] { Producto(1), Producto(2, publicado: false) };

        Assert.Equal([1], Ids(productos));
    }

    [Fact]
    public void Un_local_no_ve_productos_de_otro()
    {
        var productos = new[] { Producto(1, local: 1), Producto(2, local: 2) };

        Assert.Equal([1], Ids(productos, idLocal: 1));
    }

    [Fact]
    public void Sector_y_generico_no_salen_aunque_esten_marcados()
    {
        var productos = new[] { Producto(1), Producto(2, sector: true), Producto(3, codigo: "GENERIC-1") };

        Assert.Equal([1], Ids(productos));
    }

    [Fact]
    public void Publicar_un_producto_sector_se_rechaza()
    {
        var producto = Producto(1, publicado: false, sector: true);

        Assert.Throws<ErrorNegocio>(() => ServicioCatalogoTienda.Publicar(producto, true));
        Assert.False(producto.Publicado);
    }

    [Fact]
    public void Publicar_un_generico_se_rechaza()
    {
        var producto = Producto(1, publicado: false, codigo: "generic-3");

        Assert.Throws<ErrorNegocio>(() => ServicioCatalogoTienda.Publicar(producto, true));
    }

    [Fact]
    public void Ocultar_un_producto_sector_se_permite()
    {
        var producto = Producto(1, sector: true);

        ServicioCatalogoTienda.Publicar(producto, false);

        Assert.False(producto.Publicado);
    }

    [Fact]
    public void Busca_por_nombre_sin_importar_mayusculas()
    {
        var productos = new[] { Producto(1, nombre: "Coca Cola 1.5 lt"), Producto(2, nombre: "Pepsi") };

        Assert.Equal([1], Ids(productos, busqueda: "coca"));
    }

    [Fact]
    public void Busca_por_codigo()
    {
        var productos = new[] { Producto(1, codigo: "7790895000997"), Producto(2, codigo: "7791234") };

        Assert.Equal([1], Ids(productos, busqueda: "0895"));
    }

    [Fact]
    public void Filtrar_por_varias_categorias_devuelve_los_de_cualquiera_de_ellas()
    {
        var productos = new[] { Producto(1, categoria: 1), Producto(2, categoria: 2), Producto(3, categoria: 3), Producto(4) };

        Assert.Equal([1, 2], Ids(productos, categorias: [1, 2]));
    }

    [Fact]
    public void Sin_filtro_salen_tambien_los_productos_sin_categoria()
    {
        var productos = new[] { Producto(1, categoria: 1), Producto(2) };

        Assert.Equal([1, 2], Ids(productos));
    }

    [Fact]
    public void El_filtro_por_categoria_no_trae_publicados_ocultos()
    {
        var productos = new[] { Producto(1, categoria: 1), Producto(2, categoria: 1, publicado: false) };

        Assert.Equal([1], Ids(productos, categorias: [1]));
    }

    [Fact]
    public void El_filtro_por_categoria_se_combina_con_la_busqueda()
    {
        var productos = new[] { Producto(1, categoria: 1, nombre: "Coca"), Producto(2, categoria: 1, nombre: "Pepsi"), Producto(3, categoria: 2, nombre: "Coca light") };

        Assert.Equal([1], Ids(productos, busqueda: "coca", categorias: [1]));
    }

    [Fact]
    public void Un_producto_sin_categoria_propia_filtra_por_la_de_su_grupo()
    {
        var productos = new[] { Producto(1, grupo: 4), Producto(2, grupo: 5) };
        var grupos = new[] { Grupo(4, categoria: 1), Grupo(5, categoria: 2) };

        Assert.Equal([1], Ids(productos, categorias: [1], grupos: grupos));
    }

    [Fact]
    public void La_categoria_propia_domina_a_la_del_grupo_en_el_filtro()
    {
        var productos = new[] { Producto(1, grupo: 4, categoria: 2) };
        var grupos = new[] { Grupo(4, categoria: 1) };

        Assert.Empty(Ids(productos, categorias: [1], grupos: grupos));
        Assert.Equal([1], Ids(productos, categorias: [2], grupos: grupos));
    }

    [Fact]
    public void La_tienda_solo_lista_categorias_con_productos_publicados()
    {
        var productos = new[] { Producto(1, categoria: 1), Producto(2, categoria: 2, publicado: false) };
        var categorias = new[] { Categoria(1, "Bebidas"), Categoria(2, "Limpieza"), Categoria(3, "Almacén") };

        Assert.Equal(["Bebidas"], Categorias(productos, categorias));
    }

    [Fact]
    public void La_tienda_lista_la_categoria_heredada_del_grupo()
    {
        var productos = new[] { Producto(1, grupo: 4) };
        var grupos = new[] { Grupo(4, categoria: 2) };
        var categorias = new[] { Categoria(1, "Bebidas"), Categoria(2, "Limpieza") };

        Assert.Equal(["Limpieza"], Categorias(productos, categorias, grupos));
    }

    [Fact]
    public void La_tienda_no_lista_categorias_de_otro_local()
    {
        var productos = new[] { Producto(1, categoria: 1) };
        var categorias = new[] { Categoria(1, "Bebidas", local: 2) };

        Assert.Empty(Categorias(productos, categorias));
    }

    [Fact]
    public void El_producto_de_tienda_lleva_la_categoria_efectiva()
    {
        var heredada = ServicioCatalogoTienda.AProductoTienda(Producto(1, grupo: 4), Grupo(4, categoria: 3));
        var propia = ServicioCatalogoTienda.AProductoTienda(Producto(2, grupo: 4, categoria: 7), Grupo(4, categoria: 3));

        Assert.Equal(3, heredada.IdCategoria);
        Assert.Equal(7, propia.IdCategoria);
    }

    [Fact]
    public void Ordena_por_nombre()
    {
        var productos = new[] { Producto(1, nombre: "Yerba"), Producto(2, nombre: "Arroz") };

        Assert.Equal([2, 1], Ids(productos));
    }

    [Fact]
    public void Producto_de_tienda_dice_si_hay_stock_sin_la_cantidad()
    {
        var conStock = ServicioCatalogoTienda.AProductoTienda(Producto(1, cantidad: 7), null);
        var sinStock = ServicioCatalogoTienda.AProductoTienda(Producto(2, cantidad: 0), null);

        Assert.True(conStock.HayStock);
        Assert.False(sinStock.HayStock);
        Assert.Equal(1000, conStock.Precio);
    }

    [Fact]
    public void Producto_de_tienda_no_expone_costo_ni_cantidad()
    {
        var propiedades = typeof(ProductoTienda).GetProperties().Select(p => p.Name).ToList();

        Assert.DoesNotContain("Costo", propiedades);
        Assert.DoesNotContain("Cantidad", propiedades);
        Assert.DoesNotContain("IdLocal", propiedades);
    }
}
