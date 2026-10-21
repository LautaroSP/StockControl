namespace StockControl.Dominio;

public class Local
{
    public int IdLocal { get; set; }
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; } = true;
    public string EstadoAbono { get; set; } = "al_dia";
    public DateTimeOffset? Vence { get; set; }
    public string? IdSuscripcionMp { get; set; }
    public string EstadoSuscripcion { get; set; } = "";
}
