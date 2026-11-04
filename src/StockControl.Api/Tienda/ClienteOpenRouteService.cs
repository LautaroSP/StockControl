using System.Net.Http.Json;
using System.Text.Json;

namespace StockControl.Api.Tienda;

/// <summary>
/// Recorrido en auto entre dos puntos. Si no hay clave, no responde o se agota el cupo, devuelve null
/// y el envío sigue en línea recta. El plan gratis admite 40 recorridos por minuto.
/// </summary>
public class ClienteOpenRouteService
{
    public const int MaximoPorMinuto = 40;

    private readonly HttpClient _http;
    private readonly string _clave;
    private readonly ILogger<ClienteOpenRouteService> _log;
    private static readonly object Candado = new();
    private static readonly Queue<DateTime> Marcas = new();

    public ClienteOpenRouteService(HttpClient http, IConfiguration config, ILogger<ClienteOpenRouteService> log)
    {
        _http = http;
        _clave = config["OpenRouteService:Clave"] ?? "";
        _log = log;
    }

    public async Task<decimal?> RecorridoKm(decimal latOrigen, decimal lonOrigen, decimal latDestino, decimal lonDestino, CancellationToken cancelar)
    {
        if (string.IsNullOrWhiteSpace(_clave) || !Reservar())
            return null;
        try
        {
            using var pedido = new HttpRequestMessage(HttpMethod.Post, "openrouteservice/v2/directions/driving-car");
            pedido.Headers.TryAddWithoutValidation("Authorization", _clave);
            pedido.Content = JsonContent.Create(new
            {
                coordinates = new[]
                {
                    new[] { (double)lonOrigen, (double)latOrigen },
                    new[] { (double)lonDestino, (double)latDestino }
                }
            });
            using var respuesta = await _http.SendAsync(pedido, cancelar);
            if (!respuesta.IsSuccessStatusCode)
            {
                _log.LogWarning("OpenRouteService respondió {Status}. El envío se calcula en línea recta.", (int)respuesta.StatusCode);
                return null;
            }
            return LeerKm(await respuesta.Content.ReadAsStringAsync(cancelar));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            if (cancelar.IsCancellationRequested)
                throw;
            _log.LogWarning(ex, "OpenRouteService no respondió. El envío se calcula en línea recta.");
            return null;
        }
    }

    /// <summary>Metros del primer recorrido, en kilómetros con 2 decimales. Null si la respuesta no trae distancia.</summary>
    public static decimal? LeerKm(string json)
    {
        try
        {
            using var documento = JsonDocument.Parse(json);
            if (!documento.RootElement.TryGetProperty("routes", out var rutas) || rutas.ValueKind != JsonValueKind.Array || rutas.GetArrayLength() == 0)
                return null;
            if (!rutas[0].TryGetProperty("summary", out var resumen) || !resumen.TryGetProperty("distance", out var distancia))
                return null;
            if (distancia.ValueKind != JsonValueKind.Number || !distancia.TryGetDecimal(out var metros) || metros < 0)
                return null;
            return Math.Round(metros / 1000m, 2, MidpointRounding.AwayFromZero);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static bool Reservar()
    {
        lock (Candado)
        {
            var ahora = DateTime.UtcNow;
            while (Marcas.Count > 0 && ahora - Marcas.Peek() >= TimeSpan.FromMinutes(1))
                Marcas.Dequeue();
            if (Marcas.Count >= MaximoPorMinuto)
                return false;
            Marcas.Enqueue(ahora);
            return true;
        }
    }
}
