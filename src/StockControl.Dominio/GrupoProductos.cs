namespace StockControl.Dominio;

public class GrupoProductos
{
    public int IdGrupoProducto { get; set; }
    public int IdLocal { get; set; }
    public string NombreGrupo { get; set; } = "";
    public decimal PrecioGrupo { get; set; }
    public decimal Costo { get; set; }
    public decimal Ganancia { get; set; }
    public bool GananciaIndividual { get; set; }
}
