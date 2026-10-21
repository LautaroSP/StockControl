using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using StockControl.Dominio;

namespace StockControl.Api.Pagos;

public sealed record PreferenciaMp(string Id, string UrlPago);
public sealed record PagoMp(string Estado, string Moneda, decimal Importe, string? Referencia, DateTimeOffset? Fecha);
public sealed record SuscripcionMp(string Id, string Estado, string? UrlPago);
public sealed record CobroSuscripcionMp(string IdSuscripcion, string? IdPago, string? EstadoPago, string Moneda, decimal Importe, DateTimeOffset? Fecha);

/// <summary>Checkout Pro (pago único) y Suscripciones (débito automático) con el Access Token de StockControl.</summary>
public class ClienteMercadoPago
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public ClienteMercadoPago(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    /// <summary>
    /// Checkout Pro con credencial APP_USR se abre en init_point. sandbox_init_point no sirve para esa prueba.
    /// </summary>
    public static string ElegirUrlPago(string? initPoint, string? sandboxInitPoint) =>
        !string.IsNullOrWhiteSpace(initPoint) ? initPoint : sandboxInitPoint ?? "";

    public static string ReferenciaAbono(int idAbono) => $"abono-{idAbono}";
    public static string ReferenciaLocal(int idLocal) => $"local-{idLocal}";

    public static int? IdAbono(string? referencia) =>
        referencia != null && referencia.StartsWith("abono-") && int.TryParse(referencia[6..], out var id) ? id : null;

    public async Task<PreferenciaMp> CrearPreferencia(Abono abono, string nombreLocal, string? vuelta = null)
    {
        var url = string.IsNullOrWhiteSpace(vuelta) ? UrlActivar() : vuelta;
        var json = await Enviar(HttpMethod.Post, "checkout/preferences", new
        {
            items = new[]
            {
                new { title = $"StockControl · {nombreLocal} · 1 mes", quantity = 1, unit_price = abono.Importe, currency_id = "ARS" }
            },
            external_reference = ReferenciaAbono(abono.Id),
            notification_url = $"{Url("App:UrlApi")}/mercadopago/webhook",
            back_urls = new { success = url, pending = url, failure = url },
            auto_return = "approved",
            payment_methods = new { installments = 1 }
        });
        return new PreferenciaMp(Texto(json, "id")!, ElegirUrlPago(Texto(json, "init_point"), Texto(json, "sandbox_init_point")));
    }

    public async Task<PagoMp> ObtenerPago(string id)
    {
        var json = await Enviar(HttpMethod.Get, $"v1/payments/{Uri.EscapeDataString(id)}");
        return new PagoMp(Texto(json, "status") ?? "", Texto(json, "currency_id") ?? "", Numero(json, "transaction_amount"),
            Texto(json, "external_reference"), Fecha(json, "date_approved"));
    }

    public async Task<SuscripcionMp> CrearSuscripcion(Local local, string mailPagador, DateTimeOffset inicio)
    {
        var recurrencia = new Dictionary<string, object>
        {
            ["frequency"] = 1,
            ["frequency_type"] = "months",
            ["transaction_amount"] = ServicioAbono.PrecioMensual,
            ["currency_id"] = "ARS"
        };
        if (inicio > DateTimeOffset.UtcNow.AddMinutes(5))
            recurrencia["start_date"] = inicio.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);

        var json = await Enviar(HttpMethod.Post, "preapproval", new
        {
            reason = $"StockControl · {local.Nombre}",
            external_reference = ReferenciaLocal(local.IdLocal),
            payer_email = mailPagador,
            auto_recurring = recurrencia,
            back_url = UrlActivar(),
            status = "pending"
        });
        return new SuscripcionMp(Texto(json, "id")!, Texto(json, "status") ?? "", Texto(json, "init_point"));
    }

    public async Task<SuscripcionMp> ObtenerSuscripcion(string id)
    {
        var json = await Enviar(HttpMethod.Get, $"preapproval/{Uri.EscapeDataString(id)}");
        return new SuscripcionMp(Texto(json, "id")!, Texto(json, "status") ?? "", Texto(json, "init_point"));
    }

    public async Task CancelarSuscripcion(string id) =>
        await Enviar(HttpMethod.Put, $"preapproval/{Uri.EscapeDataString(id)}", new { status = "cancelled" });

    public async Task<CobroSuscripcionMp> ObtenerCobroSuscripcion(string id)
    {
        var json = await Enviar(HttpMethod.Get, $"authorized_payments/{Uri.EscapeDataString(id)}");
        var pago = json.TryGetProperty("payment", out var p) && p.ValueKind == JsonValueKind.Object ? p : (JsonElement?)null;
        return new CobroSuscripcionMp(
            Texto(json, "preapproval_id") ?? "",
            pago is { } a ? Texto(a, "id") : null,
            pago is { } b ? Texto(b, "status") : null,
            Texto(json, "currency_id") ?? "",
            Numero(json, "transaction_amount"),
            Fecha(json, "debit_date") ?? Fecha(json, "date_created"));
    }

    private async Task<JsonElement> Enviar(HttpMethod metodo, string ruta, object? cuerpo = null)
    {
        var token = _config["MercadoPago:AccessToken"];
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Falta MercadoPago:AccessToken.");

        using var pedido = new HttpRequestMessage(metodo, ruta);
        pedido.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (cuerpo != null)
        {
            pedido.Content = JsonContent.Create(cuerpo);
            if (metodo == HttpMethod.Post)
                pedido.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());
        }

        using var respuesta = await _http.SendAsync(pedido);
        var texto = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode)
            throw new HttpRequestException($"Mercado Pago {metodo} {ruta}: {(int)respuesta.StatusCode} {texto}");
        if (string.IsNullOrWhiteSpace(texto)) return default;
        using var documento = JsonDocument.Parse(texto);
        return documento.RootElement.Clone();
    }

    private string UrlActivar() => $"{Url("App:UrlWeb")}/activar";

    private string Url(string clave)
    {
        var url = _config[clave];
        if (string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException($"Falta {clave}.");
        return url.TrimEnd('/');
    }

    private static string? Texto(JsonElement json, string propiedad) =>
        json.TryGetProperty(propiedad, out var v) && v.ValueKind != JsonValueKind.Null
            ? (v.ValueKind == JsonValueKind.String ? v.GetString() : v.GetRawText())
            : null;

    private static decimal Numero(JsonElement json, string propiedad) =>
        json.TryGetProperty(propiedad, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetDecimal() : 0m;

    private static DateTimeOffset? Fecha(JsonElement json, string propiedad) =>
        DateTimeOffset.TryParse(Texto(json, propiedad), CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha)
            ? fecha.ToUniversalTime()
            : null;
}
