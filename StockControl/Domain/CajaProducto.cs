namespace StockControl.Domain
{
    public class CajaProducto
    {
        public int IdCajaProducto { get; set; }
        public string IdCierre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal CantidadVendida { get; set; }
        public decimal? StockInicial { get; set; }
        public decimal? StockFinal { get; set; }
        public decimal? Ganancia { get; set; }
        public int EsSector { get; set; }
        public int EsGenerico { get; set; }
    }
}
