using System.Text.Json;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

public sealed record PropuestaImagen(string? Url, bool SinRespuesta)
{
    public static readonly PropuestaImagen Ninguna = new(null, false);
    public static readonly PropuestaImagen CaidaDeLaFuente = new(null, true);
}

/// <summary>Consulta de un producto por vez en Open Food Facts y descarga de su foto frontal. Siempre desde el servidor.</summary>
public class ClienteOpenFoodFacts
{
    private readonly HttpClient _http;

    public ClienteOpenFoodFacts(HttpClient http) => _http = http;

    /// <summary>Nunca lanza por la red: si la fuente no responde, devuelve <c>SinRespuesta</c> y el apartado sigue andando.</summary>
    public async Task<PropuestaImagen> Consultar(string codigoBarras, CancellationToken cancelar)
    {
        if (!ServicioImagenesTienda.EsCodigoBarras(codigoBarras)) return PropuestaImagen.Ninguna;
        try
        {
            using var respuesta = await _http.GetAsync($"api/v3/product/{codigoBarras}.json?fields=selected_images,image_front_url", cancelar);
            if (respuesta.StatusCode == System.Net.HttpStatusCode.NotFound) return PropuestaImagen.Ninguna;
            respuesta.EnsureSuccessStatusCode();
            var url = LeerImagenFrontal(await respuesta.Content.ReadAsStringAsync(cancelar));
            return url == null ? PropuestaImagen.Ninguna : new PropuestaImagen(url, false);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !cancelar.IsCancellationRequested)
        {
            return PropuestaImagen.CaidaDeLaFuente;
        }
    }

    /// <summary>Descarga con tope de peso. Solo acepta el servidor de imágenes de Open Food Facts.</summary>
    public async Task<byte[]> Descargar(string url, CancellationToken cancelar)
    {
        if (!ServicioImagenesTienda.UrlPermitida(url))
            throw new InvalidOperationException("Dirección de imagen no permitida.");
        using var respuesta = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancelar);
        respuesta.EnsureSuccessStatusCode();
        if (respuesta.Content.Headers.ContentLength > ServicioImagenesTienda.MaximoBytes)
            throw new InvalidOperationException("La imagen de origen es demasiado pesada.");
        await using var flujo = await respuesta.Content.ReadAsStreamAsync(cancelar);
        return await LeerConTope(flujo, cancelar);
    }

    public static string? LeerImagenFrontal(string json)
    {
        using var documento = JsonDocument.Parse(json);
        if (!documento.RootElement.TryGetProperty("product", out var producto)) return null;
        var url = DeSeleccionadas(producto) ?? Texto(producto, "image_front_url");
        return ServicioImagenesTienda.UrlPermitida(url) ? url : null;
    }

    private static string? DeSeleccionadas(JsonElement producto)
    {
        if (!producto.TryGetProperty("selected_images", out var seleccion)
            || !seleccion.TryGetProperty("front", out var frontal)
            || !frontal.TryGetProperty("display", out var tamano)
            || tamano.ValueKind != JsonValueKind.Object)
            return null;
        var espanol = Texto(tamano, "es");
        if (espanol != null) return espanol;
        return tamano.EnumerateObject()
            .Where(p => p.Value.ValueKind == JsonValueKind.String)
            .Select(p => p.Value.GetString())
            .FirstOrDefault(u => !string.IsNullOrWhiteSpace(u));
    }

    private static string? Texto(JsonElement objeto, string nombre) =>
        objeto.TryGetProperty(nombre, out var valor) && valor.ValueKind == JsonValueKind.String ? valor.GetString() : null;

    private static async Task<byte[]> LeerConTope(Stream flujo, CancellationToken cancelar)
    {
        using var memoria = new MemoryStream();
        var buffer = new byte[16 * 1024];
        int leidos;
        while ((leidos = await flujo.ReadAsync(buffer, cancelar)) > 0)
        {
            memoria.Write(buffer, 0, leidos);
            if (memoria.Length > ServicioImagenesTienda.MaximoBytes)
                throw new InvalidOperationException("La imagen de origen es demasiado pesada.");
        }
        return memoria.ToArray();
    }
}
