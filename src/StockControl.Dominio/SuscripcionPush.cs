namespace StockControl.Dominio;

/// <summary>Navegador o celular que recibe Web Push.</summary>
public class SuscripcionPush
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdUsuario { get; set; }
    public string Endpoint { get; set; } = "";
    public string P256dh { get; set; } = "";
    public string Auth { get; set; } = "";
    public DateTimeOffset Creado { get; set; }
}
