using StockControl.Api.Pagos;

namespace StockControl.Tests;

public class FirmaWebhookTests
{
    private const string Secreto = "secreto-de-prueba";

    private static string Firma(string idPedido, string idDato, string ts) =>
        $"ts={ts},v1={FirmaWebhook.Calcular(FirmaWebhook.Manifiesto(idPedido, idDato, ts), Secreto)}";

    [Fact]
    public void Firma_correcta_es_valida()
    {
        var firma = Firma("req-1", "123456", "1700000000");

        Assert.True(FirmaWebhook.EsValida(firma, "req-1", "123456", Secreto));
    }

    [Fact]
    public void Firma_alterada_no_es_valida()
    {
        var firma = Firma("req-1", "123456", "1700000000");

        Assert.False(FirmaWebhook.EsValida(firma, "req-1", "999999", Secreto));
        Assert.False(FirmaWebhook.EsValida(firma, "req-1", "123456", "otro-secreto"));
        Assert.False(FirmaWebhook.EsValida("ts=1700000000", "req-1", "123456", Secreto));
    }
}
