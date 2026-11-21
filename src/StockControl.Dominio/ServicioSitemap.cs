using System.Text;
using System.Text.RegularExpressions;

namespace StockControl.Dominio;

/// <summary>
/// El dominio sale del host del pedido: no se escribe en el repo.
/// Los links de pedido de la tienda llevan un código privado y no se indexan.
/// </summary>
public static partial class ServicioSitemap
{
    public static readonly string[] RutasPublicas = ["/", "/sobre-nosotros", "/contacto", "/registro", "/terminos", "/privacidad", "/arrepentimiento"];

    public static readonly string[] RutasPrivadas =
    [
        "/caja", "/productos", "/estadisticas", "/carteles", "/cuentas", "/ofertas", "/combos", "/proveedores",
        "/grupos", "/categorias", "/informes", "/cajas", "/medios-pago", "/usuarios", "/configuracion",
        "/mi-tienda", "/activar", "/admin", "/locales", "/elegir-caja", "/aceptar", "/confirmar", "/recuperar", "/vista-cliente",
        "/tienda/*/pedido/"
    ];

    public static string Base(string? host)
    {
        var h = (host ?? "").Trim().ToLowerInvariant();
        if (!HostValido().IsMatch(h))
            h = "localhost";
        var local = h.StartsWith("localhost") || h.StartsWith("127.0.0.1");
        return (local ? "http://" : "https://") + h;
    }

    public static string Sitemap(string? host)
    {
        var b = Base(host);
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        sb.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n");
        foreach (var ruta in RutasPublicas)
            sb.Append("  <url><loc>").Append(b).Append(ruta).Append("</loc></url>\n");
        sb.Append("</urlset>\n");
        return sb.ToString();
    }

    public static string Robots(string? host)
    {
        var sb = new StringBuilder("User-agent: *\nAllow: /\n");
        foreach (var ruta in RutasPrivadas)
            sb.Append("Disallow: ").Append(ruta).Append('\n');
        sb.Append("Sitemap: ").Append(Base(host)).Append("/sitemap.xml\n");
        return sb.ToString();
    }

    [GeneratedRegex("^[a-z0-9.-]{1,253}(:[0-9]{1,5})?$")]
    private static partial Regex HostValido();
}
