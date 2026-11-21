namespace StockControl.Dominio;

public class ComprobanteAsociado
{
    public int Tipo { get; init; }
    public int PuntoVenta { get; init; }
    public long Numero { get; init; }
}

public class SolicitudArca
{
    public string Cuit { get; init; } = "";
    public int PuntoVenta { get; init; }
    /// <summary>Código ARCA (11 Factura C, 13 Nota de crédito C).</summary>
    public int Tipo { get; init; }
    public long Numero { get; init; }
    public decimal Importe { get; init; }
    public DateOnly Fecha { get; init; }
    public ComprobanteAsociado? Asociado { get; init; }
}

public class RespuestaArca
{
    public bool Aprobado { get; init; }
    public long Numero { get; init; }
    public string Cae { get; init; } = "";
    public DateOnly? VencimientoCae { get; init; }
    public string Observacion { get; init; } = "";
}

/// <summary>Lo que el dominio necesita de ARCA (WSFEv1). La implementación real vive en el API; los tests usan un falso.</summary>
public interface IClienteArca
{
    Task<long> UltimoAutorizado(string cuit, int puntoVenta, int tipo, CancellationToken ct = default);

    Task<RespuestaArca> Solicitar(SolicitudArca solicitud, CancellationToken ct = default);

    /// <summary>Null si ARCA no tiene ese número emitido.</summary>
    Task<RespuestaArca?> Consultar(string cuit, int puntoVenta, int tipo, long numero, CancellationToken ct = default);
}
