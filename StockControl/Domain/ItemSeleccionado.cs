namespace StockControl.Domain
{
    public class ItemSeleccionado
    {
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public decimal Precio { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Subtotal => Cantidad * Precio;

        public int IdProducto { get; set; }

    }
}
