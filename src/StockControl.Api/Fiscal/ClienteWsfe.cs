using System.Globalization;
using System.Xml.Linq;
using StockControl.Dominio;

namespace StockControl.Api.Fiscal;

/// <summary>
/// WSFEv1 real: FECompUltimoAutorizado, FECAESolicitar, FECompConsultar. Escrito contra la especificación de ARCA y
/// NUNCA probado contra homologación: hay que verificar el orden de los campos (p. ej. CondicionIVAReceptorId) y el tope.
/// </summary>
public sealed class ClienteWsfe : IClienteArca
{
    private const string Espacio = "http://ar.gov.afip.dif.FEV1/";
    private readonly IHttpClientFactory _clientes;
    private readonly OpcionesArca _opciones;
    private readonly ClienteWsaa _wsaa;

    public ClienteWsfe(IHttpClientFactory clientes, OpcionesArca opciones, ClienteWsaa wsaa)
    {
        _clientes = clientes;
        _opciones = opciones;
        _wsaa = wsaa;
    }

    public async Task<long> UltimoAutorizado(string cuit, int puntoVenta, int tipo, CancellationToken ct = default)
    {
        var cuerpo = $"<ar:FECompUltimoAutorizado>{await Auth(cuit, ct)}<ar:PtoVta>{puntoVenta}</ar:PtoVta><ar:CbteTipo>{tipo}</ar:CbteTipo></ar:FECompUltimoAutorizado>";
        var xml = await Llamar("FECompUltimoAutorizado", cuerpo, ct);
        VerificarErrores(xml);
        return long.Parse(Valor(xml, "CbteNro") ?? "0", CultureInfo.InvariantCulture);
    }

    public async Task<RespuestaArca> Solicitar(SolicitudArca s, CancellationToken ct = default)
    {
        var xml = await Llamar("FECAESolicitar", await ArmarSolicitud(s, ct), ct);
        VerificarErrores(xml, soloSiNoHayResultado: true);
        var aprobado = Valor(xml, "Resultado") == "A";
        return new RespuestaArca
        {
            Aprobado = aprobado,
            Numero = s.Numero,
            Cae = Valor(xml, "CAE") ?? "",
            VencimientoCae = Fecha(Valor(xml, "CAEFchVto")),
            Observacion = aprobado ? "" : Mensajes(xml)
        };
    }

    public async Task<RespuestaArca?> Consultar(string cuit, int puntoVenta, int tipo, long numero, CancellationToken ct = default)
    {
        var cuerpo = $"<ar:FECompConsultar>{await Auth(cuit, ct)}<ar:FeCompConsReq><ar:CbteTipo>{tipo}</ar:CbteTipo><ar:CbteNro>{numero}</ar:CbteNro><ar:PtoVta>{puntoVenta}</ar:PtoVta></ar:FeCompConsReq></ar:FECompConsultar>";
        var xml = await Llamar("FECompConsultar", cuerpo, ct);
        if (Valor(xml, "CodAutorizacion") is not { Length: > 0 } cae)
            return null; // 602: el comprobante no existe
        return new RespuestaArca
        {
            Aprobado = Valor(xml, "Resultado") == "A",
            Numero = numero,
            Cae = cae,
            VencimientoCae = Fecha(Valor(xml, "FchVto"))
        };
    }

    public static string CuerpoSolicitud(SolicitudArca s, string auth)
    {
        var importe = s.Importe.ToString("0.00", CultureInfo.InvariantCulture);
        var asociado = s.Asociado == null ? "" :
            $"<ar:CbtesAsoc><ar:CbteAsoc><ar:Tipo>{s.Asociado.Tipo}</ar:Tipo><ar:PtoVta>{s.Asociado.PuntoVenta}</ar:PtoVta><ar:Nro>{s.Asociado.Numero}</ar:Nro><ar:Cuit>{s.Cuit}</ar:Cuit></ar:CbteAsoc></ar:CbtesAsoc>";
        return $"<ar:FECAESolicitar>{auth}<ar:FeCAEReq><ar:FeCabReq><ar:CantReg>1</ar:CantReg><ar:PtoVta>{s.PuntoVenta}</ar:PtoVta><ar:CbteTipo>{s.Tipo}</ar:CbteTipo></ar:FeCabReq>"
            + $"<ar:FeDetReq><ar:FECAEDetRequest><ar:Concepto>1</ar:Concepto><ar:DocTipo>99</ar:DocTipo><ar:DocNro>0</ar:DocNro>"
            + $"<ar:CbteDesde>{s.Numero}</ar:CbteDesde><ar:CbteHasta>{s.Numero}</ar:CbteHasta><ar:CbteFch>{s.Fecha:yyyyMMdd}</ar:CbteFch>"
            + $"<ar:ImpTotal>{importe}</ar:ImpTotal><ar:ImpTotConc>0.00</ar:ImpTotConc><ar:ImpNeto>{importe}</ar:ImpNeto><ar:ImpOpEx>0.00</ar:ImpOpEx><ar:ImpTrib>0.00</ar:ImpTrib><ar:ImpIVA>0.00</ar:ImpIVA>"
            + $"<ar:MonId>PES</ar:MonId><ar:MonCotiz>1</ar:MonCotiz><ar:CondicionIVAReceptorId>5</ar:CondicionIVAReceptorId>{asociado}"
            + "</ar:FECAEDetRequest></ar:FeDetReq></ar:FeCAEReq></ar:FECAESolicitar>";
    }

    private async Task<string> ArmarSolicitud(SolicitudArca s, CancellationToken ct) =>
        CuerpoSolicitud(s, await Auth(s.Cuit, ct));

    private async Task<string> Auth(string cuit, CancellationToken ct)
    {
        var (token, sign) = await _wsaa.Obtener(ct);
        return $"<ar:Auth><ar:Token>{token}</ar:Token><ar:Sign>{sign}</ar:Sign><ar:Cuit>{cuit}</ar:Cuit></ar:Auth>";
    }

    private Task<XDocument> Llamar(string metodo, string cuerpo, CancellationToken ct)
    {
        var sobre = $"""<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ar="{Espacio}"><soapenv:Header/><soapenv:Body>{cuerpo}</soapenv:Body></soapenv:Envelope>""";
        return ClienteSoap.Enviar(_clientes, _opciones.UrlWsfe, Espacio + metodo, sobre, ct);
    }

    private static void VerificarErrores(XDocument xml, bool soloSiNoHayResultado = false)
    {
        if (soloSiNoHayResultado && Valor(xml, "Resultado") != null) return;
        var errores = xml.Descendants().Where(e => e.Name.LocalName == "Err").ToList();
        var falla = ClienteSoap.MensajeFalla(xml, "");
        if (errores.Count == 0 && falla.Length == 0) return;
        if (errores.Any(e => e.Elements().FirstOrDefault(x => x.Name.LocalName == "Code")?.Value == "600"))
            throw new ExcepcionArca("Falta delegar el servicio de facturación electrónica a StockControl en ARCA.", transitoria: false);
        throw new ExcepcionArca(Mensajes(xml) is { Length: > 0 } m ? m : falla, transitoria: false);
    }

    private static string Mensajes(XDocument xml) =>
        string.Join(" ", xml.Descendants().Where(e => e.Name.LocalName == "Msg").Select(e => e.Value));

    private static string? Valor(XDocument xml, string nombre) =>
        xml.Descendants().FirstOrDefault(e => e.Name.LocalName == nombre)?.Value;

    private static DateOnly? Fecha(string? texto) =>
        DateOnly.TryParseExact(texto, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var f) ? f : null;
}
