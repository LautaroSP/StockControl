using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using StockControl.Dominio;

namespace StockControl.Api.Fiscal;

/// <summary>
/// WSAA: pide token y sign para wsfe firmando el pedido (CMS) con el certificado de StockControl.
/// Un solo ticket compartido por todos los locales, en memoria hasta poco antes de vencer. Sin homologar.
/// </summary>
public sealed class ClienteWsaa
{
    private readonly IHttpClientFactory _clientes;
    private readonly OpcionesArca _opciones;
    private readonly SemaphoreSlim _candado = new(1, 1);
    private (string Token, string Sign, DateTimeOffset Vence)? _ticket;

    public ClienteWsaa(IHttpClientFactory clientes, OpcionesArca opciones)
    {
        _clientes = clientes;
        _opciones = opciones;
    }

    public async Task<(string Token, string Sign)> Obtener(CancellationToken ct)
    {
        await _candado.WaitAsync(ct);
        try
        {
            if (_ticket is { } t && t.Vence > DateTimeOffset.UtcNow.AddMinutes(5))
                return (t.Token, t.Sign);
            _ticket = await Pedir(ct);
            return (_ticket.Value.Token, _ticket.Value.Sign);
        }
        finally
        {
            _candado.Release();
        }
    }

    private async Task<(string, string, DateTimeOffset)> Pedir(CancellationToken ct)
    {
        if (!_opciones.Disponible)
            throw new ExcepcionArca("ARCA no está configurado en este servidor.", transitoria: false);
        var cms = Convert.ToBase64String(FirmarConOpciones(ArmarPedido(DateTimeOffset.UtcNow)));
        var sobre = $"""
            <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:wsaa="http://wsaa.view.sua.dvadac.desein.afip.gov">
            <soapenv:Header/><soapenv:Body><wsaa:loginCms><wsaa:in0>{cms}</wsaa:in0></wsaa:loginCms></soapenv:Body></soapenv:Envelope>
            """;
        var xml = await ClienteSoap.Enviar(_clientes, _opciones.UrlWsaa, "", sobre, ct);
        var respuesta = xml.Descendants().FirstOrDefault(e => e.Name.LocalName == "loginCmsReturn")?.Value;
        if (string.IsNullOrWhiteSpace(respuesta))
            throw new ExcepcionArca(ClienteSoap.MensajeFalla(xml, "WSAA no devolvió un ticket de acceso."), transitoria: false);
        var ticket = XDocument.Parse(respuesta);
        string Dato(string nombre) => ticket.Descendants().First(e => e.Name.LocalName == nombre).Value;
        return (Dato("token"), Dato("sign"), DateTimeOffset.Parse(Dato("expirationTime")));
    }

    public static byte[] ArmarPedido(DateTimeOffset ahora)
    {
        var local = ahora.ToOffset(TimeSpan.FromHours(-3));
        string F(DateTimeOffset d) => d.ToString("yyyy-MM-ddTHH:mm:sszzz");
        var tra = $"""
            <?xml version="1.0" encoding="UTF-8"?><loginTicketRequest version="1.0"><header><uniqueId>{ahora.ToUnixTimeSeconds()}</uniqueId><generationTime>{F(local.AddMinutes(-10))}</generationTime><expirationTime>{F(local.AddMinutes(10))}</expirationTime></header><service>wsfe</service></loginTicketRequest>
            """;
        return Encoding.UTF8.GetBytes(tra);
    }

    private byte[] FirmarConOpciones(byte[] contenido)
    {
        var (certificado, clave) = _opciones.Credenciales();
        return Firmar(certificado, clave, contenido);
    }

    public static byte[] Firmar(string certificadoPem, string clavePem, byte[] contenido)
    {
        using var conClave = X509Certificate2.CreateFromPem(certificadoPem, clavePem);
        // En Windows la clave de un PEM es efímera y CMS no la puede usar: se reimporta vía PFX en memoria.
        using var usable = new X509Certificate2(conClave.Export(X509ContentType.Pfx));
        var cms = new SignedCms(new ContentInfo(contenido));
        cms.ComputeSignature(new CmsSigner(usable));
        return cms.Encode();
    }
}