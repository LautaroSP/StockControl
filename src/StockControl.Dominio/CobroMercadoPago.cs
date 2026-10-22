namespace StockControl.Dominio;

public static class EstadoCobroMercadoPago
{
    public const string Esperando = "esperando";
    public const string Pagado = "pagado";
    public const string Cancelado = "cancelado";
    public const string Vencido = "vencido";
}

/// <summary>Cobro de mostrador que todavía no es una venta.</summary>
public class CobroMercadoPago
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int NroCaja { get; set; }
    public int? IdUsuario { get; set; }
    public string Modo { get; set; } = "";
    public string Estado { get; set; } = EstadoCobroMercadoPago.Esperando;
    public decimal Importe { get; set; }
    public string IdOrdenMp { get; set; } = "";
    public string QrData { get; set; } = "";
    public string PedidoJson { get; set; } = "";
    public int? IdInformeVenta { get; set; }
    public DateTimeOffset Creado { get; set; } = DateTimeOffset.UtcNow;

    public static string ReferenciaDe(int id) => $"cobro-{id}";

    public static int? IdDeReferencia(string? referencia) =>
        referencia != null && referencia.StartsWith("cobro-") && int.TryParse(referencia[6..], out var id) ? id : null;
}
