using StockControl.Dominio;

namespace StockControl.Tests;

public class BalanzaTests
{
    [Fact]
    public void Importe_redondea_a_centavos()
    {
        Assert.Equal(12.35m, Balanza.Importe(0.350m, 35.28m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100.001)]
    public void Peso_en_cero_negativo_o_mayor_a_100_no_vale(decimal kilos)
    {
        Assert.False(Balanza.PesoValido(kilos));
    }

    [Fact]
    public void Precio_por_kilo_en_cero_fuera_del_listado()
    {
        Assert.False(Balanza.EnListado(0));
        Assert.True(Balanza.EnListado(100));
        Assert.True(Balanza.EnListado(0.01m));
    }

    [Fact]
    public void Total_del_carrito_suma_importes()
    {
        var total = Balanza.TotalCarrito(new[]
        {
            (0.350m, 3500m),
            (0.210m, 8000m)
        });

        Assert.Equal(2905m, total);
    }

    [Theory]
    [InlineData("1.250 kg", 1.250)]
    [InlineData("ST,0,875", 0.875)]
    [InlineData("0,875", 0.875)]
    public void Texto_de_la_balanza_con_punto_o_coma(string texto, decimal esperado)
    {
        Assert.True(Balanza.TryParsePeso(texto, out var kilos));
        Assert.Equal(esperado, kilos);
    }

    [Fact]
    public void Texto_sin_numero_no_parsea()
    {
        Assert.False(Balanza.TryParsePeso("sin peso", out _));
    }

    [Fact]
    public void Codigo_de_producto_no_se_lee_como_peso()
    {
        Assert.False(Balanza.IntentarPeso("7790895005678", esCodigoProducto: true, out _));
        Assert.True(Balanza.IntentarPeso("0.350", esCodigoProducto: false, out var kilos));
        Assert.Equal(0.350m, kilos);
    }
}
