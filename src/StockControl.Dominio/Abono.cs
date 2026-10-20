namespace StockControl.Dominio;

public class Abono
{
    public const string Pendiente = "pendiente";
    public const string Aprobado = "aprobado";
    public const string Rechazado = "rechazado";

    public const string OrigenMp = "mp";
    public const string OrigenDebito = "debito";
    public const string OrigenAdmin = "admin";

    public int Id { get; set; }
    public int IdLocal { get; set; }
    public decimal Importe { get; set; }
    public string Estado { get; set; } = Pendiente;
    public string Origen { get; set; } = OrigenMp;
    public string? IdPreferenciaMp { get; set; }
    public string? UrlPago { get; set; }
    public string? IdPagoMp { get; set; }
    public DateTimeOffset FechaCreacion { get; set; }
    public DateTimeOffset? FechaAcreditacion { get; set; }
    public DateTimeOffset? VenceAnterior { get; set; }
    public DateTimeOffset? VenceNuevo { get; set; }
}
