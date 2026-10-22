namespace StockControl.Dominio;

/// <summary>Tokens de la cuenta de Mercado Pago del local. Van cifrados.</summary>
public class CuentaMercadoPago
{
    public int IdLocal { get; set; }
    public string TokenAcceso { get; set; } = "";
    public string TokenRenovacion { get; set; } = "";
    public DateTimeOffset Vence { get; set; }
    public string IdUsuarioMp { get; set; } = "";
}
