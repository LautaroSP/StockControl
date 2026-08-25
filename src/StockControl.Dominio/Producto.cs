namespace StockControl.Dominio;

public class Producto
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal Costo { get; set; }
    public decimal Precio { get; set; }
    public bool ProductoSector { get; set; }
    public int IdGrupoProducto { get; set; }
    public bool GananciaIndividual { get; set; }
    public decimal ValorGanancia { get; set; }
    public DateTimeOffset? FechaModificacion { get; set; }

    public bool EsGenerico => Codigo.StartsWith("GENERIC-", StringComparison.OrdinalIgnoreCase);
}
