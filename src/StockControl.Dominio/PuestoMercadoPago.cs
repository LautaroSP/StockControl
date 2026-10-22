namespace StockControl.Dominio;

/// <summary>Caja de Mercado Pago de un puesto. El QR impreso es de este puesto.</summary>
public class PuestoMercadoPago
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int NroCaja { get; set; }
    public string IdExterno { get; set; } = "";
    public string IdSucursal { get; set; } = "";
    public string UrlQr { get; set; } = "";
    public string IdLector { get; set; } = "";

    public static string IdExternoDe(int idLocal, int nroCaja) => $"L{idLocal}C{nroCaja}";

    public static string IdSucursalDe(int idLocal) => $"L{idLocal}";
}
