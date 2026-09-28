namespace StockControl.Dominio;

public class OrdenCompraLinea
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdOrdenCompra { get; set; }
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal? CantidadRecibida { get; set; }
}
