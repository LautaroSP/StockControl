namespace StockControl.Dominio;

public class OrdenCompra
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdProveedor { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public int? IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = "";
    public DateTimeOffset? FechaRecepcion { get; set; }
    public int? IdUsuarioRecepcion { get; set; }
    public string NombreUsuarioRecepcion { get; set; } = "";
    public List<OrdenCompraLinea> Lineas { get; set; } = new();

    public bool Recibida => FechaRecepcion != null;
}
