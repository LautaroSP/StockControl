namespace StockControl.Dominio;

/// <summary>Producto que le comprás a un proveedor, con su presentación (pack, caja, bulto). UnidadesPack 1 = por unidad.</summary>
public class ProductoProveedor
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdProducto { get; set; }
    public int IdProveedor { get; set; }
    public string NombrePack { get; set; } = "";
    public int UnidadesPack { get; set; } = 1;
}
