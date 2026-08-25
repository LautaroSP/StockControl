namespace StockControl.Dominio;

public class Local
{
    public int IdLocal { get; set; }
    public string Nombre { get; set; } = "";
    public string EstadoAbono { get; set; } = "al_dia";
    public DateTimeOffset? Vence { get; set; }
}
