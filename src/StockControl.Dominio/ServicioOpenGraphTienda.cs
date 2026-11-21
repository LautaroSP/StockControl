using System.Net;

namespace StockControl.Dominio;

/// <summary>HTML mínimo con Open Graph para los rastreadores (WhatsApp no corre JavaScript). Las personas siguen al SPA.</summary>
public static class ServicioOpenGraphTienda
{
    public static string Html(string nombre, string urlTienda, string? urlLogo)
    {
        var titulo = WebUtility.HtmlEncode(nombre);
        var url = WebUtility.HtmlEncode(urlTienda);
        var imagen = string.IsNullOrWhiteSpace(urlLogo)
            ? ""
            : $"<meta property=\"og:image\" content=\"{WebUtility.HtmlEncode(urlLogo)}\">";
        return $"<!doctype html><html lang=\"es\"><head><meta charset=\"utf-8\">"
            + $"<title>{titulo}</title>"
            + $"<meta property=\"og:type\" content=\"website\">"
            + $"<meta property=\"og:title\" content=\"{titulo}\">"
            + $"<meta property=\"og:description\" content=\"Pedí online en {titulo}\">"
            + $"<meta property=\"og:url\" content=\"{url}\">"
            + imagen
            + $"<meta http-equiv=\"refresh\" content=\"0;url={url}\">"
            + $"</head><body><a href=\"{url}\">{titulo}</a></body></html>";
    }
}
