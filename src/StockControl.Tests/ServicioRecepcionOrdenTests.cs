using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioRecepcionOrdenTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 28, 15, 0, 0, TimeSpan.Zero);

    private static OrdenCompra Orden(params OrdenCompraLinea[] lineas) =>
        new() { Id = 1, IdProveedor = 7, Lineas = lineas.ToList() };

    private static OrdenCompraLinea Linea(int id, int idProducto, decimal cantidad) =>
        new() { Id = id, IdOrdenCompra = 1, IdProducto = idProducto, Cantidad = cantidad };

    private static ResultadoRecepcion Recibir(OrdenCompra orden, IEnumerable<LineaRecibida> recibidas, params Producto[] productos) =>
        ServicioRecepcionOrden.Recibir(orden, recibidas, productos, 3, "socio", Ahora);

    [Fact]
    public void Recibir_suma_stock_con_la_cantidad_recibida()
    {
        var yerba = new Producto { Id = 10, Codigo = "A1", Cantidad = 4m };
        var orden = Orden(Linea(1, 10, 12m));

        var resultado = Recibir(orden, new[] { new LineaRecibida { IdLinea = 1, Cantidad = 10m } }, yerba);

        Assert.Equal(14m, yerba.Cantidad);
        Assert.Equal(1, resultado.ProductosActualizados);
    }

    [Fact]
    public void Cantidad_recibida_por_defecto_es_la_pedida()
    {
        var yerba = new Producto { Id = 10, Codigo = "A1", Cantidad = 0m };
        var orden = Orden(Linea(1, 10, 12m));

        Recibir(orden, Array.Empty<LineaRecibida>(), yerba);

        Assert.Equal(12m, orden.Lineas[0].CantidadRecibida);
        Assert.Equal(12m, yerba.Cantidad);
    }

    [Fact]
    public void Recibir_no_pisa_lo_pedido_y_guarda_usuario_y_fecha()
    {
        var yerba = new Producto { Id = 10, Codigo = "A1" };
        var orden = Orden(Linea(1, 10, 12m));

        Recibir(orden, new[] { new LineaRecibida { IdLinea = 1, Cantidad = 8m } }, yerba);

        Assert.Equal(12m, orden.Lineas[0].Cantidad);
        Assert.Equal(8m, orden.Lineas[0].CantidadRecibida);
        Assert.True(orden.Recibida);
        Assert.Equal(Ahora, orden.FechaRecepcion);
        Assert.Equal(3, orden.IdUsuarioRecepcion);
        Assert.Equal("socio", orden.NombreUsuarioRecepcion);
    }

    [Fact]
    public void Orden_recibida_no_se_recibe_de_nuevo()
    {
        var yerba = new Producto { Id = 10, Codigo = "A1", Cantidad = 0m };
        var orden = Orden(Linea(1, 10, 12m));
        Recibir(orden, Array.Empty<LineaRecibida>(), yerba);

        Assert.Throws<ErrorNegocio>(() => Recibir(orden, Array.Empty<LineaRecibida>(), yerba));
        Assert.Equal(12m, yerba.Cantidad);
    }

    [Fact]
    public void Cantidad_recibida_negativa_da_error()
    {
        var orden = Orden(Linea(1, 10, 12m));

        Assert.Throws<ErrorNegocio>(() =>
            Recibir(orden, new[] { new LineaRecibida { IdLinea = 1, Cantidad = -1m } }, new Producto { Id = 10 }));
        Assert.False(orden.Recibida);
    }

    [Fact]
    public void Producto_sector_o_generico_no_suma_stock()
    {
        var sector = new Producto { Id = 10, Codigo = "S1", ProductoSector = true, Cantidad = 0m };
        var generico = new Producto { Id = 11, Codigo = "GENERIC-1", Cantidad = 0m };
        var orden = Orden(Linea(1, 10, 5m), Linea(2, 11, 5m));

        var resultado = Recibir(orden, Array.Empty<LineaRecibida>(), sector, generico);

        Assert.Equal(0m, sector.Cantidad);
        Assert.Equal(0m, generico.Cantidad);
        Assert.Equal(0, resultado.ProductosActualizados);
    }

    [Fact]
    public void Linea_de_otra_orden_da_error()
    {
        var orden = Orden(Linea(1, 10, 12m));

        Assert.Throws<ErrorNegocio>(() =>
            Recibir(orden, new[] { new LineaRecibida { IdLinea = 99, Cantidad = 1m } }, new Producto { Id = 10 }));
    }

    [Fact]
    public void Linea_sin_producto_se_saltea_y_se_informa()
    {
        var orden = Orden(Linea(1, 10, 12m));

        var resultado = Recibir(orden, Array.Empty<LineaRecibida>());

        Assert.Equal(1, resultado.LineasSinProducto);
        Assert.True(orden.Recibida);
    }
}
