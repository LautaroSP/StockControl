using System.Text.RegularExpressions;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

/// <summary>
/// Imágenes en un volumen fuera de git y fuera de la base. El nombre es el hash: nunca hay dos archivos iguales.
/// Las de la API (Open Food Facts) son un caché compartido, nombradas por el hash de su dirección.
/// Las que sube un dueño van aparte, por local, nombradas por el hash de su contenido.
/// </summary>
public sealed partial class AlmacenImagenes
{
    [GeneratedRegex("^[0-9a-f]{32,64}\\.webp$")]
    private static partial Regex FormatoNombre();

    private const string CarpetaApi = "api";
    private const string CarpetaPropias = "propias";

    private readonly string _raiz;

    public AlmacenImagenes(string raiz)
    {
        _raiz = Path.GetFullPath(raiz);
        Directory.CreateDirectory(_raiz);
    }

    /// <summary>Nombre del archivo ya descargado de esa dirección, o null si hay que bajarlo.</summary>
    public string? DeApi(string url)
    {
        var nombre = ServicioImagenesTienda.NombreDeUrl(url);
        return File.Exists(RutaDe(CarpetaApi, nombre)) ? nombre : null;
    }

    public async Task<string> GuardarDeApi(string url, byte[] datos, CancellationToken cancelar)
    {
        var nombre = ServicioImagenesTienda.NombreDeUrl(url);
        await Escribir(RutaDe(CarpetaApi, nombre), datos, cancelar);
        return nombre;
    }

    public async Task<string> GuardarPropia(int idLocal, byte[] datos, CancellationToken cancelar)
    {
        var nombre = ServicioImagenesTienda.NombreDeContenido(datos);
        await Escribir(RutaDe(Path.Combine(CarpetaPropias, idLocal.ToString()), nombre), datos, cancelar);
        return nombre;
    }

    /// <summary>Null si el nombre no tiene el formato que genera el servidor (nada de rutas ajenas).</summary>
    public string? Ruta(ImagenProducto imagen)
    {
        if (!FormatoNombre().IsMatch(imagen.Archivo)) return null;
        var ruta = RutaDe(CarpetaDe(imagen), imagen.Archivo);
        if (File.Exists(ruta)) return ruta;
        var anterior = Path.Combine(_raiz, imagen.Archivo);
        return File.Exists(anterior) ? anterior : null;
    }

    /// <summary>Solo se borra lo propio y sin uso: el caché de la API se queda para la próxima vez.</summary>
    public void BorrarPropia(ImagenProducto imagen)
    {
        if (imagen.Origen != ServicioImagenesTienda.OrigenPropia) return;
        var ruta = Ruta(imagen);
        if (ruta != null) File.Delete(ruta);
    }

    private static string CarpetaDe(ImagenProducto imagen) =>
        imagen.Origen == ServicioImagenesTienda.OrigenPropia
            ? Path.Combine(CarpetaPropias, imagen.IdLocal.ToString())
            : CarpetaApi;

    private string RutaDe(string carpeta, string nombre) => Path.Combine(_raiz, carpeta, nombre);

    private static async Task Escribir(string ruta, byte[] datos, CancellationToken cancelar)
    {
        if (File.Exists(ruta)) return;
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        await File.WriteAllBytesAsync(ruta, datos, cancelar);
    }
}
