namespace StockControl.Domain
{
    public class PagoProveedor
    {
        public int IdPagoProveedor { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Proveedor { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
    }
}
