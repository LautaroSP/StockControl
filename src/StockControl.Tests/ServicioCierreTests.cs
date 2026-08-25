using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCierreTests
{
    private static InformeVenta Venta(string medio, decimal total, int? nro = null) =>
        new()
        {
            IdInformeVenta = 1,
            IdLocal = 1,
            MetodoPago = medio,
            Total = total,
            NroCaja = nro
        };

    [Fact]
    public void Cerrar_agrupa_por_medio_y_agrega_total()
    {
        var ventas = new List<InformeVenta>
        {
            Venta("Efectivo", 100),
            Venta("Efectivo", 50),
            Venta("Mercado Pago", 80)
        };
        var r = ServicioCierre.Cerrar(ventas, 3, 1, 9, "matias", DateTimeOffset.UtcNow);

        Assert.Equal(3, r.NroCaja);
        Assert.Equal(3, r.Filas.Count);
        Assert.Equal(2, r.Filas.First(f => f.MetodoPago == "Efectivo").CantidadVentas);
        Assert.Equal(150, r.Filas.First(f => f.MetodoPago == "Efectivo").Total);
        var total = r.Filas.Single(f => f.MetodoPago == "Total");
        Assert.Equal(3, total.CantidadVentas);
        Assert.Equal(230, total.Total);
        Assert.All(ventas, v => Assert.Equal(3, v.NroCaja));
    }

    [Fact]
    public void Cerrar_vacio_falla()
    {
        var ex = Assert.Throws<ErrorNegocio>(() =>
            ServicioCierre.Cerrar([], 1, 1, 9, "matias", DateTimeOffset.UtcNow));
        Assert.Contains("No hay ventas", ex.Message);
    }

    [Fact]
    public void Cerrar_no_reincluir_ventas_ya_cerradas()
    {
        var ventas = new List<InformeVenta>
        {
            Venta("Efectivo", 100, nro: 1),
            Venta("Efectivo", 40)
        };
        var r = ServicioCierre.Cerrar(ventas, 2, 1, 9, "matias", DateTimeOffset.UtcNow);

        Assert.Equal(1, r.Filas.Single(f => f.MetodoPago == "Total").CantidadVentas);
        Assert.Equal(40, r.Filas.Single(f => f.MetodoPago == "Total").Total);
        Assert.Equal(1, ventas[0].NroCaja);
        Assert.Equal(2, ventas[1].NroCaja);
    }

    [Fact]
    public void Segunda_vez_las_mismas_ventas_no_entran()
    {
        var ventas = new List<InformeVenta> { Venta("Efectivo", 100) };
        ServicioCierre.Cerrar(ventas, 1, 1, 9, "matias", DateTimeOffset.UtcNow);
        Assert.Throws<ErrorNegocio>(() =>
            ServicioCierre.Cerrar(ventas, 2, 1, 9, "matias", DateTimeOffset.UtcNow));
    }
}
