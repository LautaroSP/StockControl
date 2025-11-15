namespace StockControl.Domain
{
    public class GrupoProductos
    {
        public int IdGrupoProducto { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public decimal Costo { get; set; }
        public decimal PrecioGrupo { get; set; }
        public decimal Ganancia {  get; set; }
        public int GananciaIndividual { get; set; }
    }
}
