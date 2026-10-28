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
    public string? AliasTienda { get; set; }
    public string EstadoTienda { get; set; } = ServicioTienda.Inactiva;
    public string Direccion { get; set; } = "";
    public string Horario { get; set; } = "";
    public string WhatsApp { get; set; } = "";
    public bool HaceEnvios { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
}
