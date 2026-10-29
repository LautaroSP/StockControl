namespace StockControl.Dominio;

/// <summary>Aviso de la campanita. Uno por usuario: cada uno lo lee por su cuenta.</summary>
public class Aviso
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdUsuario { get; set; }
    public string Tipo { get; set; } = "";
    public int IdPedidoTienda { get; set; }
    public string Texto { get; set; } = "";
    public DateTimeOffset Creado { get; set; }
    public DateTimeOffset? Leido { get; set; }
}
