using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

/// <summary>Vista previa del link de la tienda. El proxy manda acá a los rastreadores (WhatsApp, etc.).</summary>
public static class EndpointsOpenGraphTienda
{
    public static void MapOpenGraphTienda(this WebApplication app) =>
        app.MapGet("/compartir/tienda/{alias}", Ver).RequireRateLimiting("tienda");

    static async Task<IResult> Ver(string alias, HttpContext http, AppDbContext db, AlcanceDatos alcance, SesionActual sesion)
    {
        var local = await LocalesTienda.PorAlias(db, alcance, sesion, alias);
        var ahora = DateTimeOffset.UtcNow;
        if (local == null || ServicioTienda.Vista(local, ahora, await ConsultaGestionTienda.Vencida(db, local.IdLocal, ahora)) == null)
            return Results.NotFound();
        var host = http.Request.Host.Value;
        var esquema = http.Request.Host.Host == "localhost" ? "http" : "https";
        var logo = local.LogoTienda == null ? null : $"{esquema}://{host}/api/tienda/{local.AliasTienda}/logo";
        var html = ServicioOpenGraphTienda.Html(local.Nombre, $"{esquema}://{host}/tienda/{local.AliasTienda}", logo);
        http.Response.Headers.CacheControl = "public, max-age=300";
        return Results.Content(html, "text/html; charset=utf-8");
    }
}
