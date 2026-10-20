using System.Security.Cryptography;
using System.Text;

namespace StockControl.Api.Pagos;

/// <summary>Valida el header x-signature de los avisos de Mercado Pago (ts=...,v1=HMAC-SHA256).</summary>
public static class FirmaWebhook
{
    public static bool EsValida(string? firma, string? idPedido, string? idDato, string secreto)
    {
        if (string.IsNullOrWhiteSpace(firma) || string.IsNullOrWhiteSpace(secreto)) return false;
        string? ts = null, v1 = null;
        foreach (var parte in firma.Split(','))
        {
            var kv = parte.Split('=', 2);
            if (kv.Length != 2) continue;
            if (kv[0].Trim() == "ts") ts = kv[1].Trim();
            if (kv[0].Trim() == "v1") v1 = kv[1].Trim();
        }
        if (ts == null || v1 == null) return false;

        var esperado = Encoding.UTF8.GetBytes(Calcular(Manifiesto(idPedido, idDato, ts), secreto));
        return CryptographicOperations.FixedTimeEquals(esperado, Encoding.UTF8.GetBytes(v1.ToLowerInvariant()));
    }

    public static string Manifiesto(string? idPedido, string? idDato, string ts)
    {
        var texto = new StringBuilder();
        if (!string.IsNullOrEmpty(idDato)) texto.Append($"id:{idDato.ToLowerInvariant()};");
        if (!string.IsNullOrEmpty(idPedido)) texto.Append($"request-id:{idPedido};");
        texto.Append($"ts:{ts};");
        return texto.ToString();
    }

    public static string Calcular(string manifiesto, string secreto)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secreto), Encoding.UTF8.GetBytes(manifiesto));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
