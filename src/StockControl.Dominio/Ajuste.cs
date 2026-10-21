namespace StockControl.Dominio;

public class Ajuste
{
    public const string TelefonoVentas = "TelefonoVentas";
    public const string EmailVentas = "EmailVentas";
    public const string FormularioContacto = "FormularioContacto";

    public string Clave { get; set; } = "";
    public string Valor { get; set; } = "";
}
