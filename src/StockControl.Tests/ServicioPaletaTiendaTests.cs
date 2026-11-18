using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioPaletaTiendaTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Vacio_vuelve_a_la_paleta_de_stockcontrol(string? color)
    {
        var local = new Local { ColorMarca = "#112233" };

        ServicioPaletaTienda.Asignar(local, color, null);

        Assert.Null(local.ColorMarca);
        Assert.Null(local.ColorFondo);
    }

    [Fact]
    public void Hex_valido_se_guarda_en_minusculas()
    {
        var local = new Local();

        ServicioPaletaTienda.Asignar(local, "#2A6B52", null);

        Assert.Equal("#2a6b52", local.ColorMarca);
    }

    [Theory]
    [InlineData("#fff")]
    [InlineData("rojo")]
    [InlineData("#12345")]
    [InlineData("#gggggg")]
    public void Hex_invalido_se_rechaza(string color)
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioPaletaTienda.Normalizar(color));

        Assert.Contains("hexadecimales", error.Message);
    }

    [Fact]
    public void Color_ilegible_con_texto_blanco_se_rechaza()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioPaletaTienda.Normalizar("#ffff00"));

        Assert.Contains("no se lee", error.Message);
    }

    [Fact]
    public void Fondo_vacio_vuelve_al_crema()
    {
        var local = new Local { ColorFondo = "#112233" };

        ServicioPaletaTienda.Asignar(local, null, "  ");

        Assert.Null(local.ColorFondo);
    }

    [Fact]
    public void Fondo_claro_se_guarda_y_el_texto_queda_oscuro()
    {
        var local = new Local();

        ServicioPaletaTienda.Asignar(local, null, "#F4EFE4");

        Assert.Equal("#f4efe4", local.ColorFondo);
        Assert.False(ServicioPaletaTienda.TextoClaro(local.ColorFondo));
    }

    [Fact]
    public void Fondo_oscuro_lleva_texto_claro()
    {
        Assert.True(ServicioPaletaTienda.TextoClaro(ServicioPaletaTienda.NormalizarFondo("#1d4a38")));
    }

    [Fact]
    public void Fondo_del_medio_que_no_se_lee_se_rechaza()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioPaletaTienda.NormalizarFondo("#808080"));

        Assert.Contains("fondo", error.Message);
    }

    [Fact]
    public void Ficha_vacia_vuelve_a_la_clara()
    {
        var local = new Local { ColorFicha = "#112233" };

        ServicioPaletaTienda.Asignar(local, null, null, "  ");

        Assert.Null(local.ColorFicha);
    }

    [Fact]
    public void Ficha_oscura_lleva_texto_claro()
    {
        var local = new Local();

        ServicioPaletaTienda.Asignar(local, null, null, "#1D4A38");

        Assert.Equal("#1d4a38", local.ColorFicha);
        Assert.True(ServicioPaletaTienda.TextoClaro(local.ColorFicha));
    }

    [Fact]
    public void Ficha_del_medio_que_no_se_lee_se_rechaza()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioPaletaTienda.NormalizarFicha("#808080"));

        Assert.Contains("ficha", error.Message);
    }
}
