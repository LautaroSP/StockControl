namespace StockControl.Dominio;

public class MetodoPago
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public string Descripcion { get; set; } = "";
    public bool Activo { get; set; } = true;
    /// <summary>manual, qr_impreso, qr_pantalla o point.</summary>
    public string Integracion { get; set; } = IntegracionPago.Manual;
}
