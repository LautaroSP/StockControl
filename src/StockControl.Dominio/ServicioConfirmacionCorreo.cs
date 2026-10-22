using System.Security.Cryptography;
using System.Text;

namespace StockControl.Dominio;

public static class ServicioConfirmacionCorreo
{
    public const int HorasDeVigencia = 48;

    public static bool HayQueConfirmar(string? apiKey, string? desde, string? urlWeb) =>
        !string.IsNullOrWhiteSpace(apiKey)
        && !string.IsNullOrWhiteSpace(desde)
        && !string.IsNullOrWhiteSpace(urlWeb);

    public static string NuevoToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    public static string? Rechazo(string? token, string? hashGuardado, DateTimeOffset? vence, DateTimeOffset ahora)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(hashGuardado) || vence == null
            || !string.Equals(Hash(token), hashGuardado, StringComparison.Ordinal))
            return "El enlace no sirve. Registrate de nuevo.";
        if (ahora > vence)
            return "El enlace venció. Registrate de nuevo.";
        return null;
    }

    public static void Confirmar(Usuario usuario)
    {
        usuario.EmailConfirmado = true;
        usuario.TokenConfirmacion = null;
        usuario.VenceConfirmacion = null;
    }
}
