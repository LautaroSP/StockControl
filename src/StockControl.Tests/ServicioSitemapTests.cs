using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioSitemapTests
{
    [Fact]
    public void Sitemap_usa_urls_absolutas_con_https_del_host()
    {
        var xml = ServicioSitemap.Sitemap("stockcontrol.com.ar");
        Assert.Contains("<loc>https://stockcontrol.com.ar/</loc>", xml);
        Assert.Contains("<loc>https://stockcontrol.com.ar/contacto</loc>", xml);
        Assert.Contains("<loc>https://stockcontrol.com.ar/sobre-nosotros</loc>", xml);
        Assert.DoesNotContain("<loc>/", xml);
    }

    [Fact]
    public void Sitemap_no_incluye_el_login()
    {
        Assert.DoesNotContain("/login<", ServicioSitemap.Sitemap("stockcontrol.com.ar"));
    }

    [Fact]
    public void Local_usa_http()
    {
        Assert.Equal("http://localhost:4200", ServicioSitemap.Base("localhost:4200"));
    }

    [Fact]
    public void Host_raro_no_entra_al_xml()
    {
        var xml = ServicioSitemap.Sitemap("evil.com\"><script>");
        Assert.DoesNotContain("<script>", xml);
        Assert.Contains("http://localhost/", xml);
    }

    [Fact]
    public void Robots_bloquea_el_panel_y_los_links_de_pedido()
    {
        var robots = ServicioSitemap.Robots("stockcontrol.com.ar");
        Assert.Contains("Disallow: /caja\n", robots);
        Assert.Contains("Disallow: /tienda/*/pedido/\n", robots);
        Assert.DoesNotContain("Disallow: /tienda/\n", robots);
        Assert.Contains("Sitemap: https://stockcontrol.com.ar/sitemap.xml", robots);
    }
}
