namespace StockControl.Dominio;

/// <summary>Un producto suelto o un grupo (exactamente uno de los dos ids).</summary>
public class AumentoHistorialItem
{
    public int Id { get; set; }
    public int IdAumentoHistorial { get; set; }
    public int IdLocal { get; set; }
    public int? IdProducto { get; set; }
    public int? IdGrupoProducto { get; set; }
    public string Nombre { get; set; } = "";
    public decimal PrecioAntes { get; set; }
    public decimal PrecioDespues { get; set; }
    public decimal CostoAntes { get; set; }
    public decimal CostoDespues { get; set; }
}
