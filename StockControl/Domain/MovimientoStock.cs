namespace StockControl.Domain
{
    public class MovimientoStock
    {
        public int IdMovimientoStock { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Codigo { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
    }
}
