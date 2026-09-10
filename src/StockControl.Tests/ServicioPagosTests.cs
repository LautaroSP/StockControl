using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioPagosTests
{
    [Fact]
    public void Pagos_multiples_deben_sumar_el_total()
    {
        var pagos = new[]
        {
            new PagoSolicitado(1, 60),
            new PagoSolicitado(2, 40)
        };

        ServicioPagos.Validar(100, pagos);
    }

    [Fact]
    public void Pagos_multiples_que_no_suman_el_total_fallan()
    {
        var pagos = new[] { new PagoSolicitado(1, 60), new PagoSolicitado(2, 39) };

        var ex = Assert.Throws<ErrorNegocio>(() => ServicioPagos.Validar(100, pagos));

        Assert.Contains("sumar exactamente", ex.Message);
    }

    [Fact]
    public void Importe_de_pago_no_puede_ser_cero()
    {
        var pagos = new[] { new PagoSolicitado(1, 0) };

        Assert.Throws<ErrorNegocio>(() => ServicioPagos.Validar(100, pagos));
    }
}
