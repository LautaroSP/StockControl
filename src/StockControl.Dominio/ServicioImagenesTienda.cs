namespace StockControl.Dominio;

/// <summary>Lo que la tienda pública muestra de una imagen: id para pedirla y atribución. Sin ruta de disco.</summary>
public sealed record ImagenTienda(int Id, string Origen, string Fuente, string Enlace, string Licencia);

public static class ServicioImagenesTienda
{
    public const int MaximoPorProducto = 2;
    public const int LadoMaximo = 400;
    public const int MaximoBytes = 2 * 1024 * 1024;
    public const int MaximoPixeles = 25_000_000;
    /// <summary>El nombre hoy mide 69 (SHA-256 en hexadecimal + ".webp"); la columna deja margen.</summary>
    public const int LargoMaximoArchivo = 100;

    public const string OrigenPropia = "propia";
    public const string OrigenOpenFoodFacts = "openfoodfacts";

    public const string Jpeg = "image/jpeg";
    public const string Png = "image/png";
    public const string WebP = "image/webp";

    public const string FuenteOpenFoodFacts = "Open Food Facts";
    public const string LicenciaOpenFoodFacts = "CC-BY-SA";

    private static readonly string[] HostsOpenFoodFacts = ["images.openfoodfacts.org", "images.openfoodfacts.net"];

    /// <summary>Base64 de lo que sube el dueño (acepta el prefijo data:image/...;base64,). Corta antes de decodificar lo que ya es demasiado.</summary>
    public static byte[] Decodificar(string? imagen)
    {
        var texto = imagen ?? "";
        var coma = texto.IndexOf(',');
        if (texto.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && coma > 0)
            texto = texto[(coma + 1)..];
        texto = texto.Trim();
        if (texto.Length > MaximoBytes / 3 * 4 + 8)
            throw new ErrorNegocio("La imagen no puede superar 2 MB.");
        try
        {
            return Convert.FromBase64String(texto);
        }
        catch (FormatException)
        {
            throw new ErrorNegocio("El archivo no es válido.");
        }
    }

    /// <summary>Nombre de archivo por contenido: la misma imagen siempre cae en el mismo archivo.</summary>
    public static string NombreDeContenido(byte[] datos) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(datos)).ToLowerInvariant() + ".webp";

    /// <summary>Nombre de archivo por dirección: una imagen de la API ya descargada no se baja de nuevo.</summary>
    public static string NombreDeUrl(string url) =>
        NombreDeContenido(System.Text.Encoding.UTF8.GetBytes(url));

    public static void ValidarOrden(int orden)
    {
        if (orden is < 1 or > MaximoPorProducto)
            throw new ErrorNegocio($"Cada producto admite hasta {MaximoPorProducto} imágenes.");
    }

    /// <summary>Tipo por los bytes, no por el nombre ni por lo que diga el cliente.</summary>
    public static string TipoReal(byte[]? datos)
    {
        if (datos == null || datos.Length == 0)
            throw new ErrorNegocio("La imagen está vacía.");
        if (datos.Length > MaximoBytes)
            throw new ErrorNegocio("La imagen no puede superar 2 MB.");
        if (EsPng(datos)) return Png;
        if (EsJpeg(datos)) return Jpeg;
        if (EsWebP(datos)) return WebP;
        throw new ErrorNegocio("La imagen debe ser JPG, PNG o WebP.");
    }

    public static void ValidarPixeles(int ancho, int alto)
    {
        if (ancho <= 0 || alto <= 0)
            throw new ErrorNegocio("La imagen no tiene medidas válidas.");
        if ((long)ancho * alto > MaximoPixeles)
            throw new ErrorNegocio("La imagen es demasiado grande.");
    }

    /// <summary>Entra en 400 × 400 conservando la proporción. Una imagen más chica no se agranda.</summary>
    public static (int Ancho, int Alto) Dimensiones(int ancho, int alto)
    {
        if (ancho <= LadoMaximo && alto <= LadoMaximo) return (ancho, alto);
        var escala = Math.Min((double)LadoMaximo / ancho, (double)LadoMaximo / alto);
        return (Math.Max(1, (int)Math.Round(ancho * escala)), Math.Max(1, (int)Math.Round(alto * escala)));
    }

    public static bool EsCodigoBarras(string? codigo)
    {
        var texto = codigo ?? "";
        return texto.Length is >= 8 and <= 14 && texto.All(char.IsAsciiDigit);
    }

    /// <summary>Solo un producto publicado con código de barras recibe propuesta.</summary>
    public static bool PuedeProponer(Producto producto) =>
        producto.Publicado && ServicioCatalogoTienda.PuedePublicar(producto) && EsCodigoBarras(producto.Codigo);

    public static void ExigirPublicado(Producto producto)
    {
        if (!producto.Publicado || !ServicioCatalogoTienda.PuedePublicar(producto))
            throw new ErrorNegocio("Publicá el producto en la tienda para cargarle imágenes.");
    }

    /// <summary>Solo https y solo el servidor de imágenes de Open Food Facts: el servidor descarga, así que no se confía en otra dirección.</summary>
    public static bool UrlPermitida(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
        return uri.Scheme == Uri.UriSchemeHttps && uri.IsDefaultPort && HostsOpenFoodFacts.Contains(uri.Host.ToLowerInvariant());
    }

    public static bool EstaRechazada(string url, IEnumerable<ImagenRechazada> rechazadas) =>
        rechazadas.Any(r => r.Url == url);

    /// <summary>Null si no corresponde guardarla (dirección inválida o ya descartada).</summary>
    public static ImagenRechazada? Rechazar(Producto producto, string? url, IEnumerable<ImagenRechazada> rechazadas)
    {
        if (!UrlPermitida(url) || EstaRechazada(url!, rechazadas)) return null;
        return new ImagenRechazada { IdLocal = producto.IdLocal, IdProducto = producto.Id, Url = url! };
    }

    public static ImagenProducto NuevaPropia(Producto producto, int orden, string archivo, string tipo, DateTimeOffset ahora)
    {
        ValidarOrden(orden);
        ExigirPublicado(producto);
        return new ImagenProducto
        {
            IdLocal = producto.IdLocal, IdProducto = producto.Id, Orden = orden,
            Archivo = archivo, TipoContenido = tipo, Origen = OrigenPropia, Fecha = ahora
        };
    }

    public static ImagenProducto NuevaDeOpenFoodFacts(Producto producto, int orden, string archivo, string tipo, DateTimeOffset ahora)
    {
        ValidarOrden(orden);
        ExigirPublicado(producto);
        if (!EsCodigoBarras(producto.Codigo))
            throw new ErrorNegocio("Sin código de barras no hay propuesta de imagen.");
        return new ImagenProducto
        {
            IdLocal = producto.IdLocal, IdProducto = producto.Id, Orden = orden,
            Archivo = archivo, TipoContenido = tipo, Origen = OrigenOpenFoodFacts,
            Fuente = FuenteOpenFoodFacts,
            Enlace = $"https://world.openfoodfacts.org/product/{producto.Codigo}",
            Licencia = LicenciaOpenFoodFacts, Fecha = ahora
        };
    }

    /// <summary>Pone la imagen en su lugar. Devuelve la que reemplazó (para borrar su archivo); nunca quedan más de 2.</summary>
    public static ImagenProducto? Colocar(IList<ImagenProducto> imagenes, ImagenProducto nueva)
    {
        ValidarOrden(nueva.Orden);
        var anterior = imagenes.FirstOrDefault(i => i.Orden == nueva.Orden);
        if (anterior != null) imagenes.Remove(anterior);
        imagenes.Add(nueva);
        return anterior;
    }

    /// <summary>Una imagen se sirve solo si es de ese producto, ese local y la tienda está abierta con el producto publicado.</summary>
    public static bool Disponible(ImagenProducto imagen, Producto producto, Local local, DateTimeOffset ahora) =>
        imagen.IdProducto == producto.Id
        && imagen.IdLocal == local.IdLocal
        && producto.IdLocal == local.IdLocal
        && producto.Publicado
        && ServicioCatalogoTienda.PuedePublicar(producto)
        && ServicioTienda.Vista(local, ahora) == ServicioTienda.VistaCatalogo;

    public static ImagenTienda ATienda(ImagenProducto imagen) =>
        new(imagen.Id, imagen.Origen, imagen.Fuente, imagen.Enlace, imagen.Licencia);

    private static bool EsPng(byte[] d) =>
        d.Length >= 8 && d[0] == 0x89 && d[1] == 0x50 && d[2] == 0x4E && d[3] == 0x47
        && d[4] == 0x0D && d[5] == 0x0A && d[6] == 0x1A && d[7] == 0x0A;

    private static bool EsJpeg(byte[] d) =>
        d.Length >= 3 && d[0] == 0xFF && d[1] == 0xD8 && d[2] == 0xFF;

    private static bool EsWebP(byte[] d) =>
        d.Length >= 12 && d[0] == 'R' && d[1] == 'I' && d[2] == 'F' && d[3] == 'F'
        && d[8] == 'W' && d[9] == 'E' && d[10] == 'B' && d[11] == 'P';
}
