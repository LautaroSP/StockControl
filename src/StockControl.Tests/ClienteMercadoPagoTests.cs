using StockControl.Api.Pagos;

namespace StockControl.Tests;

public class ClienteMercadoPagoTests
{
    [Fact]
    public void Checkout_pro_abre_init_point()
    {
        var url = ClienteMercadoPago.ElegirUrlPago("https://www.mercadopago.com.ar/pago", "https://sandbox.mercadopago.com.ar/pago");

        Assert.Equal("https://www.mercadopago.com.ar/pago", url);
    }

    [Fact]
    public void Sin_init_point_usa_el_otro_link()
    {
        var url = ClienteMercadoPago.ElegirUrlPago(null, "https://sandbox.mercadopago.com.ar/pago");

        Assert.Equal("https://sandbox.mercadopago.com.ar/pago", url);
    }
}
