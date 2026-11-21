namespace StockControl.Dominio;

public static class TipoComprobanteFiscal
{
    public const string FacturaC = "FacturaC";
    public const string NotaCreditoC = "NotaCreditoC";

    /// <summary>Código de comprobante de ARCA: 11 Factura C, 13 Nota de crédito C.</summary>
    public static int Codigo(string tipo) => tipo == NotaCreditoC ? 13 : 11;
}

public static class EstadoComprobanteFiscal
{
    public const string Autorizado = "Autorizado";
    public const string Rechazado = "Rechazado";
    /// <summary>No hay respuesta confirmada de ARCA. No se reintenta sin consultar antes.</summary>
    public const string Pendiente = "Pendiente";
}

public static class AlcanceComprobanteFiscal
{
    public const string Venta = "Venta";
    public const string Caja = "Caja";
}

public class ComprobanteFiscal
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int? IdCierre { get; set; }
    public int NroCaja { get; set; }
    public string Tipo { get; set; } = TipoComprobanteFiscal.FacturaC;
    public int PuntoVenta { get; set; }
    public long Numero { get; set; }
    public decimal Importe { get; set; }
    public string Cae { get; set; } = "";
    public DateOnly? VencimientoCae { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public string Alcance { get; set; } = AlcanceComprobanteFiscal.Venta;
    /// <summary>En una nota de crédito, la factura que anula.</summary>
    public int? IdComprobanteOrigen { get; set; }
    /// <summary>En una nota de crédito, la venta anulada.</summary>
    public int? IdInformeVenta { get; set; }
    public string Estado { get; set; } = EstadoComprobanteFiscal.Pendiente;
    public string Observacion { get; set; } = "";
    public List<ComprobanteFiscalVenta> Ventas { get; set; } = new();
}
