namespace StockControl.Dominio;

/// <summary>Propuesta de imagen que el dueño descartó: no se vuelve a ofrecer.</summary>
public class ImagenRechazada
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdProducto { get; set; }
    public string Url { get; set; } = "";
}
