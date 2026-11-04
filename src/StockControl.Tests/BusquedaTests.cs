using StockControl.Dominio;

namespace StockControl.Tests;

public class BusquedaTests
{
    [Fact]
    public void Patron_escapa_porcentaje_guion_bajo_y_barra()
    {
        Assert.Equal(@"%100\%\_a\\b%", Busqueda.Patron(@"100%_a\b"));
    }

    [Fact]
    public void Patron_corta_en_ochenta_antes_de_escapar()
    {
        var texto = new string('a', 79) + "%" + "zzzz";
        Assert.Equal("%" + new string('a', 79) + @"\%" + "%", Busqueda.Patron(texto));
    }

    [Fact]
    public void Patron_vacio_es_null()
    {
        Assert.Null(Busqueda.Patron(null));
        Assert.Null(Busqueda.Patron("   "));
    }
}
