namespace StockControl.Dominio;

public class Proveedor
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public string Nombre { get; set; } = "";
    public string TipoProductos { get; set; } = "";
    public string Telefono { get; set; } = "";
    public bool Activo { get; set; } = true;
}
