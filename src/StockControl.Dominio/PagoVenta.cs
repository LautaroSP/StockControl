namespace StockControl.Dominio;

public class PagoVenta
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdInformeVenta { get; set; }
    public int IdMetodoPago { get; set; }
    public string DescripcionMetodoPago { get; set; } = "";
    public decimal Importe { get; set; }
}
