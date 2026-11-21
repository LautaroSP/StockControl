using System.Text;
using System.Text.Json;

namespace StockControl.Dominio;

/// <summary>QR de comprobante electrónico de ARCA (v1): URL con el JSON en base64.</summary>
public static class QrArca
{
    public const string UrlBase = "https://www.afip.gob.ar/fe/qr/?p=";

    public static string? Url(ConfiguracionFiscal config, ComprobanteFiscal c)
    {
        if (c.Estado != EstadoComprobanteFiscal.Autorizado || string.IsNullOrEmpty(c.Cae)) return null;
        var datos = new
        {
            ver = 1,
            fecha = c.Fecha.ToOffset(TimeSpan.FromHours(-3)).ToString("yyyy-MM-dd"),
            cuit = long.Parse(config.Cuit),
            ptoVta = c.PuntoVenta,
            tipoCmp = TipoComprobanteFiscal.Codigo(c.Tipo),
            nroCmp = c.Numero,
            importe = c.Importe,
            moneda = "PES",
            ctz = 1,
            tipoDocRec = 99,
            nroDocRec = 0,
            tipoCodAut = "E",
            codAut = long.Parse(c.Cae)
        };
        return UrlBase + Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(datos)));
    }
}
