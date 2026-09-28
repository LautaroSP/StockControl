using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioOrdenCompraTests
{
    private static OrdenCompra Armar(IEnumerable<LineaOrdenPedida> lineas, params Producto[] productos) =>
        ServicioOrdenCompra.Armar(7, 1, lineas, productos, 1, "dueno", DateTimeOffset.UtcNow);

    [Fact]
    public void Orden_sin_lineas_da_error()
    {
        Assert.Throws<ErrorNegocio>(() => Armar(Array.Empty<LineaOrdenPedida>()));
    }

    [Fact]
    public void Producto_de_otro_proveedor_no_entra_en_la_orden()
    {
        var ajeno = new Producto { Id = 1, Codigo = "A1", IdProveedor = 2 };

        Assert.Throws<ErrorNegocio>(() =>
            Armar(new[] { new LineaOrdenPedida { IdProducto = 1, Cantidad = 3 } }, ajeno));
    }

    [Fact]
    public void Orden_copia_codigo_y_nombre_del_producto()
    {
        var propio = new Producto { Id = 1, Codigo = "A1", Nombre = "Yerba", IdProveedor = 7 };

        var orden = Armar(new[] { new LineaOrdenPedida { IdProducto = 1, Cantidad = 3 } }, propio);

        var linea = Assert.Single(orden.Lineas);
        Assert.Equal("A1", linea.Codigo);
        Assert.Equal("Yerba", linea.Nombre);
        Assert.Equal(3m, linea.Cantidad);
    }

    [Fact]
    public void Armar_orden_no_mueve_stock()
    {
        var propio = new Producto { Id = 1, Codigo = "A1", Nombre = "Yerba", IdProveedor = 7, Cantidad = 4m };

        Armar(new[] { new LineaOrdenPedida { IdProducto = 1, Cantidad = 10 } }, propio);

        Assert.Equal(4m, propio.Cantidad);
    }

    [Theory]
    [InlineData("011 15 1234-5678", "5491112345678")]
    [InlineData("0351 15 123-4567", "5493511234567")]
    [InlineData("+54 9 11 1234 5678", "5491112345678")]
    public void Telefono_saca_0_y_15_y_agrega_54(string telefono, string esperado)
    {
        Assert.Equal(esperado, ServicioOrdenCompra.NormalizarTelefono(telefono));
    }

    [Fact]
    public void Telefono_corto_da_error()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioOrdenCompra.NormalizarTelefono("1234"));
    }

    [Fact]
    public void Url_de_whatsapp_lleva_el_texto_codificado()
    {
        var url = ServicioOrdenCompra.UrlWhatsApp("1112345678", "Hola & chau");

        Assert.Equal("https://wa.me/5491112345678?text=Hola%20%26%20chau", url);
    }
}
