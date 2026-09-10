using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCierreTests
{
    private static InformeVenta Venta(string medio, decimal total, int puesto = 1, int? idCierre = null, int? idUsuario = null) =>
        new()
        {
            IdInformeVenta = 1,
            IdLocal = 1,
            MetodoPago = medio,
            Total = total,
            NroCaja = puesto,
            IdCierre = idCierre,
            IdUsuario = idUsuario
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
        var r = ServicioCierre.Cerrar(ventas, 1, 10, 1, 9, "matias", DateTimeOffset.UtcNow, TipoDesgloseCaja.Medio);

        Assert.Equal(10, r.IdCierre);
        Assert.Equal(1, r.NroCaja);
        Assert.Equal(3, r.Filas.Count);
        Assert.Equal(2, r.Filas.First(f => f.MetodoPago == "Efectivo").CantidadVentas);
        Assert.Equal(150, r.Filas.First(f => f.MetodoPago == "Efectivo").Total);
        var total = r.Filas.Single(f => f.MetodoPago == "Total");
        Assert.Equal(3, total.CantidadVentas);
        Assert.Equal(230, total.Total);
        Assert.All(ventas, v => Assert.Equal(10, v.IdCierre));
        Assert.All(ventas, v => Assert.Equal(1, v.NroCaja));
        Assert.All(r.Filas, f => Assert.Equal(TipoDesgloseCaja.Medio, f.TipoDesglose));
    }

    [Fact]
    public void Cerrar_por_usuario()
    {
        var ventas = new List<InformeVenta>
        {
            Venta("Efectivo", 100, idUsuario: 1),
            Venta("Mercado Pago", 50, idUsuario: 1),
            Venta("Efectivo", 80, idUsuario: 2)
        };
        var nombres = new Dictionary<int, string> { [1] = "matias", [2] = "ana" };
        var r = ServicioCierre.Cerrar(ventas, 1, 5, 1, 9, "matias", DateTimeOffset.UtcNow, TipoDesgloseCaja.Usuario, nombres);

        Assert.Equal(3, r.Filas.Count);
        Assert.Equal(150, r.Filas.First(f => f.MetodoPago == "matias").Total);
        Assert.Equal(2, r.Filas.First(f => f.MetodoPago == "matias").CantidadVentas);
        Assert.Equal(80, r.Filas.First(f => f.MetodoPago == "ana").Total);
        Assert.All(r.Filas, f => Assert.Equal(TipoDesgloseCaja.Usuario, f.TipoDesglose));
    }

    [Fact]
    public void Cerrar_solo_el_puesto()
    {
        var ventas = new List<InformeVenta>
        {
            Venta("Efectivo", 100, puesto: 1),
            Venta("Efectivo", 40, puesto: 2)
        };
        var r = ServicioCierre.Cerrar(ventas, 1, 3, 1, 9, "matias", DateTimeOffset.UtcNow, TipoDesgloseCaja.Medio);

        Assert.Equal(1, r.Filas.Single(f => f.MetodoPago == "Total").CantidadVentas);
        Assert.Equal(100, r.Filas.Single(f => f.MetodoPago == "Total").Total);
        Assert.Equal(3, ventas[0].IdCierre);
        Assert.Null(ventas[1].IdCierre);
    }

    [Fact]
    public void Cerrar_vacio_falla()
    {
        var ex = Assert.Throws<ErrorNegocio>(() =>
            ServicioCierre.Cerrar([], 1, 1, 1, 9, "matias", DateTimeOffset.UtcNow, TipoDesgloseCaja.Medio));
        Assert.Contains("No hay ventas", ex.Message);
    }

    [Fact]
    public void Cerrar_no_reincluir_ventas_ya_cerradas()
    {
        var ventas = new List<InformeVenta>
        {
            Venta("Efectivo", 100, idCierre: 1),
            Venta("Efectivo", 40)
        };
        var r = ServicioCierre.Cerrar(ventas, 1, 2, 1, 9, "matias", DateTimeOffset.UtcNow, TipoDesgloseCaja.Medio);

        Assert.Equal(1, r.Filas.Single(f => f.MetodoPago == "Total").CantidadVentas);
        Assert.Equal(40, r.Filas.Single(f => f.MetodoPago == "Total").Total);
        Assert.Equal(1, ventas[0].IdCierre);
        Assert.Equal(2, ventas[1].IdCierre);
    }

    [Fact]
    public void Segunda_vez_las_mismas_ventas_no_entran()
    {
        var ventas = new List<InformeVenta> { Venta("Efectivo", 100) };
        ServicioCierre.Cerrar(ventas, 1, 1, 1, 9, "matias", DateTimeOffset.UtcNow, TipoDesgloseCaja.Medio);
        Assert.Throws<ErrorNegocio>(() =>
            ServicioCierre.Cerrar(ventas, 1, 2, 1, 9, "matias", DateTimeOffset.UtcNow, TipoDesgloseCaja.Medio));
    }

    [Fact]
    public void Cerrar_desglosa_una_venta_con_pagos_multiples()
    {
        var venta = Venta("Múltiple", 100);
        var pagos = new Dictionary<int, IReadOnlyList<PagoVenta>>
        {
            [1] = new[]
            {
                new PagoVenta { IdInformeVenta = 1, DescripcionMetodoPago = "Efectivo", Importe = 60 },
                new PagoVenta { IdInformeVenta = 1, DescripcionMetodoPago = "Transferencia", Importe = 40 }
            }
        };

        var r = ServicioCierre.Cerrar(
            new[] { venta }, 1, 4, 1, 9, "matias", DateTimeOffset.UtcNow,
            TipoDesgloseCaja.Medio, pagosPorVenta: pagos);

        Assert.Equal(60, r.Filas.Single(f => f.MetodoPago == "Efectivo").Total);
        Assert.Equal(40, r.Filas.Single(f => f.MetodoPago == "Transferencia").Total);
        Assert.Equal(100, r.Filas.Single(f => f.MetodoPago == "Total").Total);
    }
}
