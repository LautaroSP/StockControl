using Microsoft.EntityFrameworkCore;
using StockControl.Api.Contratos;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

public static class EndpointsLogoTienda
{
    public static void MapLogoTienda(this WebApplication app)
    {
        app.MapPut("/configuracion/tienda/logo", Subir).RequireAuthorization().RequireRateLimiting("api");
        app.MapDelete("/configuracion/tienda/logo", Quitar).RequireAuthorization().RequireRateLimiting("api");
        app.MapGet("/tienda/{alias}/logo", Ver).RequireRateLimiting("tienda-imagenes");
    }

    static async Task<IResult> Subir(ImagenProductoPedido pedido, AppDbContext db, SesionActual sesion, AlmacenImagenes almacen, CancellationToken cancelar)
    {
        var local = await LocalDe(db, sesion);
        if (local == null) return SinLocal(sesion);
        try
        {
            var preparada = ProcesadorImagen.PrepararLogo(ServicioImagenesTienda.Decodificar(pedido.Imagen));
            var archivo = await almacen.GuardarPropia(local.IdLocal, preparada.Datos, cancelar);
            var anterior = local.LogoTienda;
            ServicioLogoTienda.Poner(local, archivo);
            await db.SaveChangesAsync(cancelar);
            await Liberar(db, almacen, local.IdLocal, anterior, archivo);
            return Results.Ok(new { logo = local.LogoTienda });
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    static async Task<IResult> Quitar(AppDbContext db, SesionActual sesion, AlmacenImagenes almacen)
    {
        var local = await LocalDe(db, sesion);
        if (local == null) return SinLocal(sesion);
        var anterior = local.LogoTienda;
        ServicioLogoTienda.Quitar(local);
        await db.SaveChangesAsync();
        await Liberar(db, almacen, local.IdLocal, anterior, null);
        return Results.NoContent();
    }

    static async Task<IResult> Ver(string alias, HttpContext http, AppDbContext db, AlcanceDatos alcance, SesionActual sesion, AlmacenImagenes almacen)
    {
        var local = await LocalesTienda.PorAlias(db, alcance, sesion, alias);
        if (local?.LogoTienda == null) return Results.NotFound();
        var ruta = almacen.RutaPropia(local.IdLocal, local.LogoTienda);
        if (ruta == null) return Results.NotFound();
        http.Response.Headers.CacheControl = "public, max-age=86400";
        return Results.File(ruta, ServicioImagenesTienda.WebP);
    }

    static async Task<Local?> LocalDe(AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal == null || !Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return null;
        return await db.Locales.FirstOrDefaultAsync(l => l.IdLocal == sesion.IdLocal);
    }

    static IResult SinLocal(SesionActual sesion) =>
        sesion.IdLocal == null ? Results.BadRequest("Elegí un local.") : Results.Forbid();

    static async Task Liberar(AppDbContext db, AlmacenImagenes almacen, int idLocal, string? anterior, string? actual)
    {
        if (anterior == null || anterior == actual) return;
        var enUso = await db.ImagenesProducto.AnyAsync(i => i.IdLocal == idLocal && i.Archivo == anterior);
        if (!enUso) almacen.BorrarPropia(idLocal, anterior);
    }
}
