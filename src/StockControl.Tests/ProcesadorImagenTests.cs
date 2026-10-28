using SkiaSharp;
using StockControl.Api.Tienda;
using StockControl.Dominio;

namespace StockControl.Tests;

public class ProcesadorImagenTests
{
    private static byte[] Imagen(int ancho, int alto, SKEncodedImageFormat formato)
    {
        using var bitmap = new SKBitmap(ancho, alto);
        using var lienzo = new SKCanvas(bitmap);
        lienzo.Clear(SKColors.OrangeRed);
        using var imagen = SKImage.FromBitmap(bitmap);
        return imagen.Encode(formato, 90).ToArray();
    }

    private static (int Ancho, int Alto) Medidas(byte[] datos)
    {
        using var bitmap = SKBitmap.Decode(datos);
        return (bitmap.Width, bitmap.Height);
    }

    [Fact]
    public void Una_foto_grande_queda_en_400_de_lado_sin_deformarse()
    {
        var preparada = ProcesadorImagen.Preparar(Imagen(1600, 1200, SKEncodedImageFormat.Jpeg));

        Assert.Equal((400, 300), (preparada.Ancho, preparada.Alto));
        Assert.Equal((400, 300), Medidas(preparada.Datos));
    }

    [Fact]
    public void Una_foto_vertical_queda_con_400_de_alto()
    {
        var preparada = ProcesadorImagen.Preparar(Imagen(600, 1200, SKEncodedImageFormat.Png));

        Assert.Equal((200, 400), (preparada.Ancho, preparada.Alto));
    }

    [Fact]
    public void Una_imagen_chica_no_se_agranda()
    {
        var preparada = ProcesadorImagen.Preparar(Imagen(120, 80, SKEncodedImageFormat.Png));

        Assert.Equal((120, 80), (preparada.Ancho, preparada.Alto));
    }

    [Fact]
    public void Se_guarda_siempre_como_WebP()
    {
        var preparada = ProcesadorImagen.Preparar(Imagen(500, 500, SKEncodedImageFormat.Png));

        Assert.Equal(ServicioImagenesTienda.WebP, preparada.Tipo);
        Assert.Equal(ServicioImagenesTienda.WebP, ServicioImagenesTienda.TipoReal(preparada.Datos));
    }

    [Fact]
    public void Acepta_una_imagen_WebP()
    {
        var preparada = ProcesadorImagen.Preparar(Imagen(900, 900, SKEncodedImageFormat.Webp));

        Assert.Equal((400, 400), (preparada.Ancho, preparada.Alto));
    }

    [Fact]
    public void Un_archivo_con_firma_de_PNG_pero_roto_se_rechaza()
    {
        byte[] roto = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4, 5, 6, 7, 8];

        Assert.Throws<ErrorNegocio>(() => ProcesadorImagen.Preparar(roto));
    }

    [Fact]
    public void Un_archivo_que_no_es_imagen_se_rechaza()
    {
        var texto = System.Text.Encoding.UTF8.GetBytes("esto no es una imagen");

        Assert.Throws<ErrorNegocio>(() => ProcesadorImagen.Preparar(texto));
    }
}
