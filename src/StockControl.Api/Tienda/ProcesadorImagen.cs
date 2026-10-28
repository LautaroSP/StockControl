using SkiaSharp;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

public sealed record ImagenPreparada(byte[] Datos, string Tipo, int Ancho, int Alto);

/// <summary>Valida que el archivo sea una imagen real y la guarda en WebP, dentro de 400 × 400 y sin deformarla.</summary>
public static class ProcesadorImagen
{
    private const int Calidad = 80;

    public static ImagenPreparada Preparar(byte[] origen)
    {
        ServicioImagenesTienda.TipoReal(origen);
        using var datos = SKData.CreateCopy(origen);
        using var codec = SKCodec.Create(datos) ?? throw new ErrorNegocio("No se pudo leer la imagen.");
        ServicioImagenesTienda.ValidarPixeles(codec.Info.Width, codec.Info.Height);
        var giro = codec.EncodedOrigin;
        using var leida = SKBitmap.Decode(codec) ?? throw new ErrorNegocio("No se pudo leer la imagen.");
        using var derecha = Orientar(leida, giro);
        var (ancho, alto) = ServicioImagenesTienda.Dimensiones(derecha.Width, derecha.Height);
        using var final = ancho == derecha.Width && alto == derecha.Height
            ? derecha.Copy()
            : derecha.Resize(new SKImageInfo(ancho, alto), SKFilterQuality.High);
        if (final == null) throw new ErrorNegocio("No se pudo achicar la imagen.");
        using var imagen = SKImage.FromBitmap(final);
        using var codificada = imagen.Encode(SKEncodedImageFormat.Webp, Calidad)
            ?? throw new ErrorNegocio("No se pudo guardar la imagen.");
        return new ImagenPreparada(codificada.ToArray(), ServicioImagenesTienda.WebP, ancho, alto);
    }

    /// <summary>Las fotos de celular traen el giro en los metadatos; sin esto salen acostadas.</summary>
    private static SKBitmap Orientar(SKBitmap bitmap, SKEncodedOrigin origen)
    {
        var grados = origen switch
        {
            SKEncodedOrigin.RightTop => 90,
            SKEncodedOrigin.BottomRight => 180,
            SKEncodedOrigin.LeftBottom => 270,
            _ => 0
        };
        if (grados == 0) return bitmap.Copy();
        var girada = grados == 180
            ? new SKBitmap(bitmap.Width, bitmap.Height)
            : new SKBitmap(bitmap.Height, bitmap.Width);
        using var lienzo = new SKCanvas(girada);
        lienzo.Translate(girada.Width / 2f, girada.Height / 2f);
        lienzo.RotateDegrees(grados);
        lienzo.Translate(-bitmap.Width / 2f, -bitmap.Height / 2f);
        lienzo.DrawBitmap(bitmap, 0, 0);
        return girada;
    }
}
