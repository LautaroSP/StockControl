namespace StockControl.Dominio;

public class OrdenCompraLinea
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdOrdenCompra { get; set; }
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    /// <summary>Cantidad pedida en la presentación de la línea (packs si hay pack, unidades si no).</summary>
    public decimal Cantidad { get; set; }
    public decimal? CantidadRecibida { get; set; }
    public string NombrePack { get; set; } = "";
    public int UnidadesPack { get; set; } = 1;
}
