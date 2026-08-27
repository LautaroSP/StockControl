using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioUnificarCajasTests
{
    private static Caja Total(int idCierre, int puesto, string quien, DateTimeOffset fecha, decimal total = 100, int tickets = 1, string desglose = TipoDesgloseCaja.Medio) =>
        new()
        {
            IdLocal = 1,
            IdCierre = idCierre,
            NroCaja = puesto,
            NombreCierre = quien,
            Fecha = fecha,
            MetodoPago = "Total",
            Total = total,
            CantidadVentas = tickets,
            TipoDesglose = desglose
        };

    private static Caja Fila(int idCierre, int puesto, string medio, decimal total, int cant, DateTimeOffset fecha, string quien = "matias") =>
        new()
        {
            IdLocal = 1,
            IdCierre = idCierre,
            NroCaja = puesto,
            NombreCierre = quien,
            Fecha = fecha,
            MetodoPago = medio,
            Total = total,
            CantidadVentas = cant,
            TipoDesglose = TipoDesgloseCaja.Medio
        };

    [Fact]
    public void GruposUnificables_mismo_puesto_persona_y_dia()
    {
        var dia = new DateTimeOffset(2026, 8, 26, 15, 0, 0, TimeSpan.Zero);
        var filas = new List<Caja>
        {
            Total(1, 1, "matias", dia, 100),
            Total(2, 1, "matias", dia.AddHours(2), 50),
            Total(3, 1, "ana", dia, 80),
            Total(4, 2, "matias", dia, 40)
        };
        var g = ServicioUnificarCajas.GruposUnificables(filas);
        Assert.Single(g);
        Assert.Equal(new[] { 1, 2 }, g[0].IdsCierre);
    }

    [Fact]
    public void Unificar_suma_medios_y_total()
    {
        var f = new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero);
        var filas = new List<Caja>
        {
            Fila(1, 1, "Efectivo", 100, 2, f),
            Total(1, 1, "matias", f, 100, 2),
            Fila(2, 1, "Efectivo", 40, 1, f.AddHours(1)),
            Fila(2, 1, "Mercado Pago", 60, 1, f.AddHours(1)),
            Total(2, 1, "matias", f.AddHours(1), 100, 2)
        };
        var r = ServicioUnificarCajas.Unificar(filas, [1, 2]);
        Assert.Equal(1, r.IdCierre);
        Assert.Equal(new[] { 2 }, r.IdsEliminados);
        Assert.Equal(140, r.Filas.First(x => x.MetodoPago == "Efectivo").Total);
        Assert.Equal(3, r.Filas.First(x => x.MetodoPago == "Efectivo").CantidadVentas);
        Assert.Equal(60, r.Filas.First(x => x.MetodoPago == "Mercado Pago").Total);
        var total = r.Filas.Single(x => x.MetodoPago == "Total");
        Assert.Equal(200, total.Total);
        Assert.Equal(4, total.CantidadVentas);
    }

    [Fact]
    public void Unificar_falla_si_distinto_puesto()
    {
        var f = DateTimeOffset.UtcNow;
        var filas = new List<Caja>
        {
            Total(1, 1, "matias", f),
            Total(2, 2, "matias", f)
        };
        Assert.Throws<ErrorNegocio>(() => ServicioUnificarCajas.Unificar(filas, [1, 2]));
    }
}
