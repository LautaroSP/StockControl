namespace StockControl.Dominio;

public class MensajeContacto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Mensaje { get; set; } = "";
    public DateTimeOffset Creado { get; set; }
    public DateTimeOffset? Leido { get; set; }
}
