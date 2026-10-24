namespace StockControl.Dominio;

/// <summary>Cliente del local al que se le fía.</summary>
public class Cliente
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public string Nombre { get; set; } = "";
    public string Telefono { get; set; } = "";
}
