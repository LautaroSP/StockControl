using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCatalogoTiendaTests
{
    private static Producto Producto(int id, int local = 1, bool publicado = true, bool sector = false, string? codigo = null, string? nombre = null, int grupo = 0, decimal cantidad = 10) => new()
    {
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

    private static List<int> Ids(IEnumerable<Producto> productos, int idLocal = 1, string? busqueda = null, int? idGrupo = null) =>
        ServicioCatalogoTienda.Publicados(productos.AsQueryable(), idLocal, busqueda, idGrupo).Select(p => p.Id).ToList();

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
    public void Filtra_por_grupo_de_producto()
    {
        var productos = new[] { Producto(1, grupo: 4), Producto(2, grupo: 5), Producto(3) };

        Assert.Equal([1], Ids(productos, idGrupo: 4));
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
        var conStock = ServicioCatalogoTienda.AProductoTienda(Producto(1, cantidad: 7));
        var sinStock = ServicioCatalogoTienda.AProductoTienda(Producto(2, cantidad: 0));

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
