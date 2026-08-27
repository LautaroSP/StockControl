namespace StockControl.Dominio;

public class OcupacionCaja
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int NroCaja { get; set; }
    public int IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = "";
    public DateTimeOffset Desde { get; set; } = DateTimeOffset.UtcNow;
}
