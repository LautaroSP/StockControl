using System.Globalization;
using System.Text.Json;

namespace StockControl.Api.Tienda;

public sealed record LugarEncontrado(string Nombre, decimal Latitud, decimal Longitud);

/// <summary>Búsqueda de direcciones en Nominatim (OSM). Política: 1 consulta por segundo y User-Agent identificable.</summary>
public class ClienteNominatim
{
    public const int MaximoResultados = 5;

    private readonly HttpClient _http;

    public ClienteNominatim(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<LugarEncontrado>> Buscar(string direccion, CancellationToken cancelar)
    {
        var url = $"search?format=jsonv2&countrycodes=ar&limit={MaximoResultados}&q={Uri.EscapeDataString(direccion)}";
        using var respuesta = await _http.GetAsync(url, cancelar);
        respuesta.EnsureSuccessStatusCode();
        return LeerLugares(await respuesta.Content.ReadAsStringAsync(cancelar));
    }

    public static IReadOnlyList<LugarEncontrado> LeerLugares(string json)
    {
        using var documento = JsonDocument.Parse(json);
        if (documento.RootElement.ValueKind != JsonValueKind.Array) return [];
        var lugares = new List<LugarEncontrado>();
        foreach (var item in documento.RootElement.EnumerateArray().Take(MaximoResultados))
        {
            if (!item.TryGetProperty("lat", out var lat) || !item.TryGetProperty("lon", out var lon)) continue;
            if (!decimal.TryParse(lat.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var latitud)) continue;
            if (!decimal.TryParse(lon.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var longitud)) continue;
            var nombre = item.TryGetProperty("display_name", out var n) ? n.GetString() ?? "" : "";
            lugares.Add(new LugarEncontrado(nombre, Math.Round(latitud, 6), Math.Round(longitud, 6)));
        }
        return lugares;
    }
}
