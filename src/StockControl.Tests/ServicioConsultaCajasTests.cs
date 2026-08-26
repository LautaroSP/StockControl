using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioConsultaCajasTests
{
    private static Caja Fila(int nro, string medio, string quien = "matias", decimal total = 100) =>
        new()
        {
            IdLocal = 1,
            NroCaja = nro,
            MetodoPago = medio,
            NombreCierre = quien,
            Total = total,
            CantidadVentas = 1,
            Fecha = DateTimeOffset.UtcNow
        };

    [Fact]
    public void NrosConMedio_solo_cierres_con_ese_medio()
    {
        var filas = new List<Caja>
        {
            Fila(1, "Efectivo"),
            Fila(1, "Total", total: 100),
            Fila(2, "Mercado Pago"),
            Fila(2, "Total", total: 50)
        };
        var nros = ServicioConsultaCajas.NrosConMedio(filas, "Efectivo");
        Assert.Equal(new HashSet<int> { 1 }, nros);
    }

    [Fact]
    public void FiltrarTotales_por_quien_y_medio()
    {
        var filas = new List<Caja>
        {
            Fila(1, "Efectivo", "matias"),
            Fila(1, "Total", "matias", 100),
            Fila(2, "Efectivo", "ana"),
            Fila(2, "Total", "ana", 80),
            Fila(3, "Mercado Pago", "matias"),
            Fila(3, "Total", "matias", 40)
        };
        var r = ServicioConsultaCajas.FiltrarTotales(filas, "mati", "Efectivo");
        Assert.Single(r);
        Assert.Equal(1, r[0].NroCaja);
    }

    [Fact]
    public void MesActual_desde_uno_hasta_hoy()
    {
        var (desde, hasta) = ServicioConsultaCajas.MesActual(new DateOnly(2026, 8, 26));
        Assert.Equal(new DateOnly(2026, 8, 1), desde);
        Assert.Equal(new DateOnly(2026, 8, 26), hasta);
    }
}
