namespace StockControl.Dominio;

/// <summary>Imagen de un producto publicado. El archivo vive en disco; acá solo el nombre y la atribución.</summary>
public class ImagenProducto
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdProducto { get; set; }
    public int Orden { get; set; }
    public string Archivo { get; set; } = "";
    public string TipoContenido { get; set; } = "";
    public string Origen { get; set; } = "";
    public string Fuente { get; set; } = "";
    public string Enlace { get; set; } = "";
    public string Licencia { get; set; } = "";
    public DateTimeOffset Fecha { get; set; }
}
