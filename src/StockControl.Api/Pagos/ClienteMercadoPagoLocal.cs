using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace StockControl.Api.Pagos;

public sealed record TokenMercadoPago(string Acceso, string Renovacion, DateTimeOffset Vence, string IdUsuario);

public sealed record OrdenMercadoPago(string Id, string Estado, string? QrData);

/// <summary>Órdenes y OAuth con el token del local. No usa el token del abono.</summary>
public class ClienteMercadoPagoLocal
{
    private readonly HttpClient _http;

    public ClienteMercadoPagoLocal(HttpClient http) => _http = http;

    public static string UrlAutorizacion(string clientId, string redirect, string state) =>
        "https://auth.mercadopago.com.ar/authorization?response_type=code&platform_id=mp"
        + $"&client_id={Uri.EscapeDataString(clientId)}"
        + $"&redirect_uri={Uri.EscapeDataString(redirect)}"
        + $"&state={Uri.EscapeDataString(state)}";

    public Task<TokenMercadoPago> CanjearCodigo(string clientId, string clientSecret, string code, string redirect) =>
        PedirToken(new
        {
            client_id = clientId,
            client_secret = clientSecret,
            grant_type = "authorization_code",
            code,
            redirect_uri = redirect
        });

    public Task<TokenMercadoPago> Renovar(string clientId, string clientSecret, string refresh) =>
        PedirToken(new
        {
            client_id = clientId,
            client_secret = clientSecret,
            grant_type = "refresh_token",
            refresh_token = refresh
        });

    public async Task<string> AsegurarSucursal(string token, string idUsuario, string idExterno, string nombre)
    {
        var hallada = await BuscarSucursal(token, idUsuario, idExterno);
        if (hallada != null) return hallada;
        var json = await Enviar(token, HttpMethod.Post, $"users/{Uri.EscapeDataString(idUsuario)}/stores", new
        {
            name = nombre,
            external_id = idExterno,
            location = Ubicacion(nombre)
        });
        return Texto(json, "id") ?? throw new HttpRequestException("Mercado Pago no devolvió la sucursal.");
    }

    public async Task<(string IdExterno, string UrlQr)> AsegurarCaja(string token, string idSucursal, string idExterno, string nombre)
    {
        var existente = await BuscarCaja(token, idExterno);
        if (existente != null) return existente.Value;
        var json = await Enviar(token, HttpMethod.Post, "pos", new
        {
            name = nombre,
            fixed_amount = true,
            store_id = long.Parse(idSucursal, CultureInfo.InvariantCulture),
            external_id = idExterno,
            category = 621102
        });
        var url = "";
        if (json.TryGetProperty("qr", out var qr))
            url = Texto(qr, "template_document") ?? Texto(qr, "image") ?? "";
        return (idExterno, url);
    }

    public Task<OrdenMercadoPago> CrearOrdenQr(string token, string idExterno, string modo, string referencia, decimal importe, IReadOnlyList<(string Titulo, decimal Precio, int Cantidad)> items) =>
        CrearOrden(token, new
        {
            type = "qr",
            total_amount = Dinero(importe),
            external_reference = referencia,
            expiration_time = "PT15M",
            description = "Venta en caja",
            config = new { qr = new { external_pos_id = idExterno, mode = modo } },
            transactions = new { payments = new[] { new { amount = Dinero(importe) } } },
            items = items.Select(i => new
            {
                title = i.Titulo,
                unit_price = Dinero(i.Precio),
                quantity = i.Cantidad,
                unit_measure = "unit"
            })
        });

    public Task<OrdenMercadoPago> CrearOrdenPoint(string token, string idLector, string referencia, decimal importe) =>
        CrearOrden(token, new
        {
            type = "point",
            total_amount = Dinero(importe),
            external_reference = referencia,
            expiration_time = "PT15M",
            description = "Venta en caja",
            config = new { point = new { terminal_id = idLector, print_on_terminal = "no_ticket" } },
            transactions = new { payments = new[] { new { amount = Dinero(importe) } } }
        });

    public async Task<OrdenMercadoPago> ObtenerOrden(string token, string idOrden)
    {
        var json = await Enviar(token, HttpMethod.Get, $"v1/orders/{Uri.EscapeDataString(idOrden)}");
        return LeerOrden(json);
    }

    public Task CancelarOrden(string token, string idOrden) =>
        Enviar(token, HttpMethod.Post, $"v1/orders/{Uri.EscapeDataString(idOrden)}/cancel", new { });

    private async Task<OrdenMercadoPago> CrearOrden(string token, object cuerpo)
    {
        var json = await Enviar(token, HttpMethod.Post, "v1/orders", cuerpo);
        return LeerOrden(json);
    }

    private async Task<TokenMercadoPago> PedirToken(object cuerpo)
    {
        using var pedido = new HttpRequestMessage(HttpMethod.Post, "oauth/token")
        {
            Content = JsonContent.Create(cuerpo)
        };
        using var respuesta = await _http.SendAsync(pedido);
        var texto = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode)
            throw new HttpRequestException($"Mercado Pago no entregó el token ({(int)respuesta.StatusCode}).");
        using var documento = JsonDocument.Parse(texto);
        var json = documento.RootElement;
        var segundos = json.TryGetProperty("expires_in", out var e) && e.TryGetInt32(out var n) ? n : 15_000_000;
        return new TokenMercadoPago(
            Texto(json, "access_token") ?? "",
            Texto(json, "refresh_token") ?? "",
            DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, segundos - 120)),
            Texto(json, "user_id") ?? "");
    }

    private async Task<string?> BuscarSucursal(string token, string idUsuario, string idExterno)
    {
        var json = await EnviarSiExiste(token, HttpMethod.Get, $"users/{Uri.EscapeDataString(idUsuario)}/stores/search?external_id={Uri.EscapeDataString(idExterno)}");
        if (json == null || !json.Value.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
            return null;
        foreach (var item in results.EnumerateArray())
        {
            if (Texto(item, "external_id") == idExterno)
                return Texto(item, "id");
        }
        return null;
    }

    private async Task<(string IdExterno, string UrlQr)?> BuscarCaja(string token, string idExterno)
    {
        var json = await EnviarSiExiste(token, HttpMethod.Get, $"pos?external_id={Uri.EscapeDataString(idExterno)}");
        if (json == null || !json.Value.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
            return null;
        foreach (var item in results.EnumerateArray())
        {
            if (Texto(item, "external_id") != idExterno) continue;
            var url = "";
            if (item.TryGetProperty("qr", out var qr))
                url = Texto(qr, "template_document") ?? Texto(qr, "image") ?? "";
            return (idExterno, url);
        }
        return null;
    }

    private async Task<JsonElement?> EnviarSiExiste(string token, HttpMethod metodo, string ruta)
    {
        try
        {
            return await Enviar(token, metodo, ruta);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains(": 404 ", StringComparison.Ordinal))
        {
            return null;
        }
    }

    private async Task<JsonElement> Enviar(string token, HttpMethod metodo, string ruta, object? cuerpo = null)
    {
        using var pedido = new HttpRequestMessage(metodo, ruta);
        pedido.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (cuerpo != null)
        {
            pedido.Content = JsonContent.Create(cuerpo);
            if (metodo == HttpMethod.Post)
                pedido.Headers.TryAddWithoutValidation("X-Idempotency-Key", Guid.NewGuid().ToString());
        }
        using var respuesta = await _http.SendAsync(pedido);
        var texto = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode)
            throw new HttpRequestException($"Mercado Pago {metodo} {ruta}: {(int)respuesta.StatusCode} {texto}");
        if (string.IsNullOrWhiteSpace(texto)) return default;
        using var documento = JsonDocument.Parse(texto);
        return documento.RootElement.Clone();
    }

    private static OrdenMercadoPago LeerOrden(JsonElement json)
    {
        string? qr = null;
        if (json.TryGetProperty("type_response", out var tipo))
            qr = Texto(tipo, "qr_data");
        return new OrdenMercadoPago(Texto(json, "id") ?? "", Texto(json, "status") ?? "", qr);
    }

    /// <summary>Mercado Pago exige latitud y longitud al crear la sucursal. El spike de mañana las cambia por la dirección del local.</summary>
    private static object Ubicacion(string nombre) => new
    {
        street_number = "0",
        street_name = nombre,
        city_name = "La Plata",
        state_name = "Buenos Aires",
        latitude = -34.9214,
        longitude = -57.9544,
        reference = nombre
    };

    private static string Dinero(decimal importe) =>
        importe.ToString("0.00", CultureInfo.InvariantCulture);

    private static string? Texto(JsonElement json, string propiedad) =>
        json.TryGetProperty(propiedad, out var v) && v.ValueKind != JsonValueKind.Null
            ? (v.ValueKind == JsonValueKind.String ? v.GetString() : v.GetRawText())
            : null;
}
