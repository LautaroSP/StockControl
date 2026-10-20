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

    [Fact]
    public void Alta_sin_precio_usa_el_factor_y_el_iva()
    {
        var precio = PrecioVenta.PrecioAlCrear(1000, 0, sector: false, tieneGrupo: false, 1.5m, 1.21m);

        Assert.Equal(1815m, precio);
    }

    [Fact]
    public void Alta_con_precio_cargado_no_lo_recalcula()
    {
        var precio = PrecioVenta.PrecioAlCrear(1000, 2500, sector: false, tieneGrupo: false, 1.5m, 1.21m);

        Assert.Equal(2500m, precio);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Alta_de_sector_o_grupo_no_calcula_el_precio(bool sector, bool tieneGrupo)
    {
        var precio = PrecioVenta.PrecioAlCrear(1000, 0, sector, tieneGrupo, 1.5m, 1.21m);

        Assert.Equal(0m, precio);
    }
}
