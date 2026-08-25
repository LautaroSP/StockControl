namespace StockControl.Dominio;

public class Caja
{
    public int IdCaja { get; set; }
    public int IdLocal { get; set; }
    public int NroCaja { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = "";
    public int CantidadVentas { get; set; }
    public int? IdUsuarioCierre { get; set; }
    public string NombreCierre { get; set; } = "";
}
