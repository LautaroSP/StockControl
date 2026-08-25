namespace StockControl.Dominio;

public class InformeVentaDetalle
{
    public int IdInformeVentaDetalle { get; set; }
    public int IdLocal { get; set; }
    public int IdInformeVenta { get; set; }
    public int? IdProducto { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal? Costo { get; set; }
    public decimal Precio { get; set; }
    public decimal SubTotal { get; set; }
}
