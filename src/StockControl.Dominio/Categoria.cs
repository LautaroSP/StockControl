namespace StockControl.Dominio;

public class Categoria
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public string Nombre { get; set; } = "";
    /// <summary>Null = categoría raíz. Solo un nivel de hijos.</summary>
    public int? IdPadre { get; set; }
}
