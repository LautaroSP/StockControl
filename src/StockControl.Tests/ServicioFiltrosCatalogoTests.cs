using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioFiltrosCatalogoTests
{
    private static Producto Producto(int id, bool sector = false, int grupo = 0, decimal cantidad = 10) => new()
    {
        Id = id,
        IdLocal = 1,
        Nombre = $"Producto {id}",
        ProductoSector = sector,
        IdGrupoProducto = grupo,
        Cantidad = cantidad
    };

    [Fact]
    public void Filtrar_por_grupo_devuelve_solo_sus_productos()
    {
        var productos = new[] { Producto(1, grupo: 2), Producto(2, grupo: 3) };

        var resultado = ServicioFiltrosCatalogo.Filtrar(productos, null, 2, false, false, 5);

        Assert.Single(resultado);
        Assert.Equal(1, resultado.Single().Id);
    }

    [Fact]
    public void Filtrar_sin_grupo_devuelve_productos_sin_grupo()
    {
        var productos = new[] { Producto(1), Producto(2, grupo: 1) };

        var resultado = ServicioFiltrosCatalogo.Filtrar(productos, null, null, true, false, 5);

        Assert.Equal(1, resultado.Single().Id);
    }

    [Fact]
    public void Stock_bajo_ignora_productos_sector()
    {
        var productos = new[] { Producto(1, cantidad: 2), Producto(2, sector: true, cantidad: 1) };

        var resultado = ServicioFiltrosCatalogo.Filtrar(productos, null, null, false, true, 5);

        Assert.Single(resultado);
        Assert.Equal(1, resultado.Single().Id);
    }
}
