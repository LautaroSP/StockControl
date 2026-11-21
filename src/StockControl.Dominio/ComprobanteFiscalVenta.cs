namespace StockControl.Dominio;

/// <summary>Una venta incluida en una factura. Una venta solo puede estar en una factura.</summary>
public class ComprobanteFiscalVenta
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdComprobanteFiscal { get; set; }
    public int IdInformeVenta { get; set; }
    public decimal Importe { get; set; }
}
