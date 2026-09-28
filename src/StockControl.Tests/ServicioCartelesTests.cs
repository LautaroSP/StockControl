using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCartelesTests
{
    private static Producto Prod(int id, decimal precio = 100) =>
        new() { Id = id, Codigo = $"P{id}", Nombre = $"Producto {id}", Precio = precio };

    [Fact]
    public void Grande_entran_6_por_hoja_y_chico_30()
    {
        Assert.Equal(6, ServicioCarteles.CartelesPorHoja("grande"));
        Assert.Equal(30, ServicioCarteles.CartelesPorHoja("chico"));
    }

    [Fact]
    public void Siete_grandes_ocupan_dos_hojas()
    {
        var productos = Enumerable.Range(1, 7).Select(i => Prod(i));

        var hojas = ServicioCarteles.ArmarHojas(productos, "grande");

        Assert.Equal(2, hojas.Count);
        Assert.Equal(6, hojas[0].Count);
        Assert.Single(hojas[1]);
    }

    [Fact]
    public void Precio_del_cartel_va_sin_decimales()
    {
        var hojas = ServicioCarteles.ArmarHojas([Prod(1, 800.99m)], "chico");

        Assert.Equal(800m, hojas[0][0].Precio);
    }

    [Fact]
    public void Tamano_desconocido_da_error()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioCarteles.NormalizarTamano("mediano"));
    }

    [Fact]
    public void Plantilla_que_no_es_png_ni_jpg_se_rechaza()
    {
        var gif = "GIF89a"u8.ToArray();

        Assert.Throws<ErrorNegocio>(() => ServicioCarteles.ValidarPlantilla(gif));
    }

    [Fact]
    public void Plantilla_de_mas_de_1_MB_se_rechaza()
    {
        var grande = new byte[ServicioCarteles.MaxBytesPlantilla + 1];
        grande[0] = 0xFF; grande[1] = 0xD8; grande[2] = 0xFF;

        Assert.Throws<ErrorNegocio>(() => ServicioCarteles.ValidarPlantilla(grande));
    }

    [Fact]
    public void Plantilla_png_valida_devuelve_su_tipo()
    {
        byte[] png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];

        Assert.Equal("image/png", ServicioCarteles.ValidarPlantilla(png));
    }
}
