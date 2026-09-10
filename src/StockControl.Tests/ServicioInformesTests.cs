using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioInformesTests
{
    private static InformeVenta Venta(int id, int puesto, int? idCierre = null) =>
        new()
        {
            IdInformeVenta = id,
            NroCaja = puesto,
            IdCierre = idCierre,
            Total = 100
        };

    [Fact]
    public void Filtrar_por_puesto_deja_solo_ese()
    {
        var ventas = new[] { Venta(1, 1), Venta(2, 2), Venta(3, 1) };

        var r = ServicioInformes.Filtrar(ventas, nroCaja: 1, idCierre: null).ToList();

        Assert.Equal(new[] { 1, 3 }, r.Select(v => v.IdInformeVenta));
    }

    [Fact]
    public void Filtrar_por_cierre_deja_las_ventas_de_esa_caja()
    {
        var ventas = new[] { Venta(1, 1, 10), Venta(2, 1, 11), Venta(3, 2, 10) };

        var r = ServicioInformes.Filtrar(ventas, nroCaja: null, idCierre: 10).ToList();

        Assert.Equal(new[] { 1, 3 }, r.Select(v => v.IdInformeVenta));
    }

    [Fact]
    public void Sin_filtros_devuelve_todas()
    {
        var ventas = new[] { Venta(1, 1), Venta(2, 2) };

        Assert.Equal(2, ServicioInformes.Filtrar(ventas, null, null).Count());
    }
}
