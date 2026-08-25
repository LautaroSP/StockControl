namespace StockControl.Dominio;

public class InformeVenta
{
    public int IdInformeVenta { get; set; }
    public int IdLocal { get; set; }
    public int? IdUsuario { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = "";
    public bool MultipleMetodoDePago { get; set; }
    public bool DetalleAdjunto { get; set; } = true;
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
    public string PrecioCosto { get; set; } = "NO";
    public int? NroCaja { get; set; }
    public List<InformeVentaDetalle> Detalles { get; set; } = new();
}
