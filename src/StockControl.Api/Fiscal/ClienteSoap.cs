using System.Text;
using System.Xml.Linq;
using StockControl.Dominio;

namespace StockControl.Api.Fiscal;

/// <summary>Envío SOAP 1.1 a ARCA. Caída y timeout se marcan transitorios: no sabemos si ARCA llegó a emitir.</summary>
internal static class ClienteSoap
{
    public const string Cliente = "arca";

    public static async Task<XDocument> Enviar(IHttpClientFactory clientes, string url, string accion, string sobre, CancellationToken ct)
    {
        try
        {
            using var http = clientes.CreateClient(Cliente);
            using var pedido = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(sobre, Encoding.UTF8, "text/xml")
            };
            pedido.Headers.Add("SOAPAction", accion);
            using var respuesta = await http.SendAsync(pedido, ct);
            var texto = await respuesta.Content.ReadAsStringAsync(ct);
            return XDocument.Parse(texto);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Xml.XmlException)
        {
            throw new ExcepcionArca("ARCA no respondió.", transitoria: true);
        }
    }

    public static string MensajeFalla(XDocument xml, string porDefecto)
    {
        var falla = xml.Descendants().FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value;
        return string.IsNullOrWhiteSpace(falla) ? porDefecto : falla;
    }
}
