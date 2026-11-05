using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioQrTiendaTests
{
    [Fact]
    public void Los_tres_tamanos_crecen_en_la_hoja_y_en_la_descarga()
    {
        Assert.Equal(80, ServicioQrTienda.LadoMm(" chico "));
        Assert.Equal(120, ServicioQrTienda.LadoMm("MEDIANO"));
        Assert.Equal(160, ServicioQrTienda.LadoMm("grande"));
        Assert.True(ServicioQrTienda.LadoPx("chico") < ServicioQrTienda.LadoPx("mediano"));
        Assert.True(ServicioQrTienda.LadoPx("mediano") < ServicioQrTienda.LadoPx("grande"));
    }

    [Fact]
    public void En_la_pos_el_qr_tiene_un_solo_lado()
    {
        Assert.Equal(50, ServicioQrTienda.LadoPosMm);
    }

    [Fact]
    public void Tamano_desconocido_da_error()
    {
        var ex = Assert.Throws<ErrorNegocio>(() => ServicioQrTienda.NormalizarTamano("enorme"));

        Assert.Equal("El tamaño del QR debe ser chico, mediano o grande.", ex.Message);
    }

    [Fact]
    public void Sin_alias_no_hay_qr()
    {
        var ex = Assert.Throws<ErrorNegocio>(() => ServicioQrTienda.RutaComprador("  "));

        Assert.Equal("La tienda no tiene alias. Sin alias no hay QR.", ex.Message);
    }

    [Fact]
    public void La_ruta_lleva_al_comprador_de_la_tienda()
    {
        Assert.Equal("/tienda/lo-de-pepe", ServicioQrTienda.RutaComprador("Lo-de-Pepe"));
    }

    [Fact]
    public void La_pos_de_58_es_mas_angosta_y_el_ticket_a4_sale_en_80()
    {
        Assert.Equal(58, ServicioQrTienda.AnchoPapelPosMm("POS-58"));
        Assert.Equal(80, ServicioQrTienda.AnchoPapelPosMm("POS-80"));
        Assert.Equal(80, ServicioQrTienda.AnchoPapelPosMm("A4"));
    }

    [Fact]
    public void La_marca_impresa_es_StockControl()
    {
        Assert.Equal("StockControl", ServicioQrTienda.Marca);
    }
}
