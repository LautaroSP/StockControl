using StockControl.Dominio;

namespace StockControl.Tests;

public class PrecioVentaTests
{
    [Fact]
    public void Calcular_lista_aplica_factor_e_iva()
    {
        var precio = PrecioVenta.CalcularLista(100, 1.5m, 1.21m);

        Assert.Equal(181.50m, precio);
    }
}
