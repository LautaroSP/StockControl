namespace StockControl.Dominio;

public class Caja
{
    public int IdCaja { get; set; }
    public int IdLocal { get; set; }
    /// <summary>Puesto (1…N).</summary>
    public int NroCaja { get; set; }
    /// <summary>Identifica un cierre (varias filas comparten el mismo).</summary>
    public int IdCierre { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = "";
    public int CantidadVentas { get; set; }
    public int? IdUsuarioCierre { get; set; }
    public string NombreCierre { get; set; } = "";
    /// <summary>Medio o Usuario.</summary>
    public string TipoDesglose { get; set; } = TipoDesgloseCaja.Medio;
}

public static class TipoDesgloseCaja
{
    public const string Medio = "Medio";
    public const string Usuario = "Usuario";
}
