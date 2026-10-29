using System.Security.Cryptography;
using Lib.Net.Http.WebPush.Authentication;

namespace StockControl.Api.Avisos;

/// <summary>Claves VAPID de Web Push. Van en el entorno, nunca en git.</summary>
public static class ClavesVapid
{
    public static string? Publica(IConfiguration config) =>
        Autenticacion(config) == null ? null : config["WebPush:ClavePublica"];

    public static VapidAuthentication? Autenticacion(IConfiguration config)
    {
        var publica = config["WebPush:ClavePublica"];
        var privada = config["WebPush:ClavePrivada"];
        var asunto = config["WebPush:Asunto"];
        if (string.IsNullOrWhiteSpace(publica) || string.IsNullOrWhiteSpace(privada) || string.IsNullOrWhiteSpace(asunto))
            return null;
        try
        {
            return new VapidAuthentication(publica, privada) { Subject = asunto };
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>Par nuevo en base64url: pública sin comprimir (65 bytes) y privada (32 bytes).</summary>
    public static (string Publica, string Privada) Generar()
    {
        using var clave = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var p = clave.ExportParameters(true);
        var publica = new byte[65];
        publica[0] = 0x04;
        p.Q.X!.CopyTo(publica, 1);
        p.Q.Y!.CopyTo(publica, 33);
        return (Base64Url(publica), Base64Url(p.D!));
    }

    private static string Base64Url(byte[] datos) =>
        Convert.ToBase64String(datos).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
