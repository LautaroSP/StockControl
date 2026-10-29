namespace StockControl.Dominio;

/// <summary>Alta de la suscripción Web Push. El servidor después le escribe a ese endpoint: solo servicios de push conocidos.</summary>
public static class ServicioPush
{
    public const int LargoMaximoEndpoint = 500;
    public const int MaximoPorUsuario = 10;

    private static readonly string[] Servicios =
    [
        "fcm.googleapis.com",
        "updates.push.services.mozilla.com",
        "push.apple.com",
        "notify.windows.com"
    ];

    public static SuscripcionPush Crear(int idLocal, int idUsuario, string? endpoint, string? p256dh, string? auth, DateTimeOffset ahora) => new()
    {
        IdLocal = idLocal,
        IdUsuario = idUsuario,
        Endpoint = ValidarEndpoint(endpoint),
        P256dh = ValidarClave(p256dh, 80, 100),
        Auth = ValidarClave(auth, 16, 30),
        Creado = ahora
    };

    public static string ValidarEndpoint(string? endpoint)
    {
        var valor = (endpoint ?? "").Trim();
        if (valor.Length > LargoMaximoEndpoint
            || !Uri.TryCreate(valor, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || !uri.IsDefaultPort
            || !Servicios.Any(s => uri.Host == s || uri.Host.EndsWith("." + s, StringComparison.Ordinal)))
            throw new ErrorNegocio("Ese navegador no tiene un servicio de avisos compatible.");
        return valor;
    }

    private static string ValidarClave(string? clave, int minimo, int maximo)
    {
        var valor = (clave ?? "").Trim();
        if (valor.Length < minimo || valor.Length > maximo || !valor.All(EsBase64Url))
            throw new ErrorNegocio("La suscripción del navegador no es válida.");
        return valor;
    }

    private static bool EsBase64Url(char c) => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '=';
}
