namespace StockControl.Dominio;

/// <summary>Deuda de gestión de un pedido de la tienda que no pasó por Mercado Pago. Una fila por pedido.</summary>
public class GestionTienda
{
    public const string Pendiente = "pendiente";
    public const string Saldada = "saldada";
    public const string Anulada = "anulada";

    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdPedido { get; set; }
    public decimal Importe { get; set; }
    public string Medio { get; set; } = "";
    public string Estado { get; set; } = Pendiente;
    public int? IdAbono { get; set; }
    public DateTimeOffset Nacida { get; set; }
    public DateTimeOffset? FechaSaldo { get; set; }
    /// <summary>Vencimiento del abono del período en que nació. Pagar el mes después no lo corre.</summary>
    public DateTimeOffset VencePeriodo { get; set; }
}
