namespace StockControl.Dominio;

public class Arrepentimiento
{
    public const string Pendiente = "pendiente";

    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Correo { get; set; } = "";
    public string Local { get; set; } = "";
    public string Mensaje { get; set; } = "";
    public string Estado { get; set; } = Pendiente;
    public DateTimeOffset Creado { get; set; }
    /// <summary>Null: es del abono de StockControl y lo ve el admin. Con local: es de la tienda.</summary>
    public int? IdLocal { get; set; }
}
