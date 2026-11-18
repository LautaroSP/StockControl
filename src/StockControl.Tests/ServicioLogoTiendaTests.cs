using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioLogoTiendaTests
{
    [Fact]
    public void Una_imagen_chica_no_se_agranda()
    {
        Assert.Equal((80, 40), ServicioLogoTienda.Dimensiones(80, 40));
    }

    [Fact]
    public void Una_imagen_grande_entra_en_200_sin_deformarse()
    {
        Assert.Equal((200, 100), ServicioLogoTienda.Dimensiones(400, 200));
        Assert.Equal((200, 200), ServicioLogoTienda.Dimensiones(800, 800));
    }

    [Fact]
    public void Poner_guarda_el_archivo_y_quitar_lo_deja_vacio()
    {
        var local = new Local();

        ServicioLogoTienda.Poner(local, "abc.webp");
        Assert.Equal("abc.webp", local.LogoTienda);

        ServicioLogoTienda.Quitar(local);
        Assert.Null(local.LogoTienda);
    }
}
