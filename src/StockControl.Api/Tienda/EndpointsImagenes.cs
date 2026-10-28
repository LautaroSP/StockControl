using Microsoft.EntityFrameworkCore;
using StockControl.Api.Contratos;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

/// <summary>Imágenes de producto para el dueño y el socio: ver, proponer, aceptar, subir y borrar.</summary>
public static class EndpointsImagenes
{
    private const string ErrorFuente = "No pudimos consultar Open Food Facts. Podés subir tu propia imagen.";

    public static void MapImagenesProducto(this WebApplication app)
    {
        app.MapGet("/productos/{id:int}/imagenes", Listar).RequireAuthorization().RequireRateLimiting("api");
        app.MapGet("/productos/{id:int}/imagenes/propuesta", Proponer).RequireAuthorization().RequireRateLimiting("openfoodfacts");
        app.MapPost("/productos/{id:int}/imagenes/propuesta/aceptar", Aceptar).RequireAuthorization().RequireRateLimiting("openfoodfacts");
        app.MapPut("/productos/{id:int}/imagenes/{orden:int}", Subir).RequireAuthorization().RequireRateLimiting("api");
        app.MapDelete("/productos/{id:int}/imagenes/{orden:int}", Borrar).RequireAuthorization().RequireRateLimiting("api");
    }

    private static async Task<IResult> Listar(int id, AppDbContext db, SesionActual sesion, AlmacenImagenes almacen)
    {
        var (producto, error) = await ProductoDelLocal(id, db, sesion);
        if (producto == null) return error!;
        var imagenes = await db.ImagenesProducto.AsNoTracking().Where(i => i.IdProducto == id).OrderBy(i => i.Orden).ToListAsync();
        return Results.Ok(new
        {
            publicado = producto.Publicado,
            maximo = ServicioImagenesTienda.MaximoPorProducto,
            imagenes = imagenes.Select(i => new
            {
                orden = i.Orden, origen = i.Origen, fuente = i.Fuente, enlace = i.Enlace, licencia = i.Licencia,
                datos = DataUrl(almacen, i)
            })
        });
    }

    private static async Task<IResult> Proponer(int id, AppDbContext db, SesionActual sesion, ClienteOpenFoodFacts fuente, CancellationToken cancelar)
    {
        var (producto, error) = await ProductoDelLocal(id, db, sesion);
        if (producto == null) return error!;
        var propuesta = ServicioImagenesTienda.PuedeProponer(producto)
            ? await Propuesta(producto, db, fuente, cancelar)
            : PropuestaImagen.Ninguna;
        return Results.Ok(new
        {
            disponible = propuesta.Url != null,
            sinRespuesta = propuesta.SinRespuesta,
            url = propuesta.Url ?? "",
            fuente = ServicioImagenesTienda.FuenteOpenFoodFacts,
            enlace = EnlaceDe(producto),
            licencia = ServicioImagenesTienda.LicenciaOpenFoodFacts
        });
    }

    private static async Task<IResult> Aceptar(int id, AceptarImagenPedido pedido, AppDbContext db, SesionActual sesion, ClienteOpenFoodFacts fuente, AlmacenImagenes almacen, ILoggerFactory logs, CancellationToken cancelar)
    {
        var (producto, error) = await ProductoDelLocal(id, db, sesion);
        if (producto == null) return error!;
        try
        {
            ServicioImagenesTienda.ValidarOrden(pedido.Orden);
            if (!ServicioImagenesTienda.PuedeProponer(producto))
                throw new ErrorNegocio("Este producto no tiene propuesta de imagen.");
            var propuesta = await Propuesta(producto, db, fuente, cancelar);
            if (propuesta.SinRespuesta) return Results.Problem(ErrorFuente, statusCode: StatusCodes.Status502BadGateway);
            var url = propuesta.Url ?? throw new ErrorNegocio("No hay una imagen propuesta para este producto.");
            var archivo = almacen.DeApi(url) ?? await DescargarYGuardar(url, fuente, almacen, cancelar);
            await Guardar(db, almacen,
                ServicioImagenesTienda.NuevaDeOpenFoodFacts(producto, pedido.Orden, archivo, ServicioImagenesTienda.WebP, DateTimeOffset.UtcNow), cancelar);
            return Results.Ok(new { orden = pedido.Orden });
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException or System.Text.Json.JsonException)
        {
            logs.CreateLogger("OpenFoodFacts").LogWarning(ex, "Falló la descarga de la imagen propuesta");
            return Results.Problem(ErrorFuente, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static async Task<IResult> Subir(int id, int orden, ImagenProductoPedido pedido, AppDbContext db, SesionActual sesion, AlmacenImagenes almacen, CancellationToken cancelar)
    {
        var (producto, error) = await ProductoDelLocal(id, db, sesion);
        if (producto == null) return error!;
        try
        {
            ServicioImagenesTienda.ValidarOrden(orden);
            ServicioImagenesTienda.ExigirPublicado(producto);
            var preparada = ProcesadorImagen.Preparar(ServicioImagenesTienda.Decodificar(pedido.Imagen));
            await DescartarPropuesta(db, producto, pedido.Rechazada);
            var archivo = await almacen.GuardarPropia(producto.IdLocal, preparada.Datos, cancelar);
            await Guardar(db, almacen,
                ServicioImagenesTienda.NuevaPropia(producto, orden, archivo, preparada.Tipo, DateTimeOffset.UtcNow), cancelar);
            return Results.Ok(new { orden });
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> Borrar(int id, int orden, AppDbContext db, SesionActual sesion, AlmacenImagenes almacen)
    {
        var (producto, error) = await ProductoDelLocal(id, db, sesion);
        if (producto == null) return error!;
        var imagen = await db.ImagenesProducto.FirstOrDefaultAsync(i => i.IdProducto == id && i.Orden == orden);
        if (imagen == null) return Results.NotFound();
        db.ImagenesProducto.Remove(imagen);
        await db.SaveChangesAsync();
        await LiberarPropia(db, almacen, imagen);
        return Results.NoContent();
    }

    private static async Task<(Producto? Producto, IResult? Error)> ProductoDelLocal(int id, AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal == null) return (null, Results.BadRequest("Elegí un local."));
        if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return (null, Results.Forbid());
        var producto = await db.Productos.FirstOrDefaultAsync(p => p.Id == id);
        return producto == null ? (null, Results.NotFound()) : (producto, null);
    }

    /// <summary>La dirección de la foto frontal, salvo que no exista o el dueño ya la haya descartado.</summary>
    private static async Task<PropuestaImagen> Propuesta(Producto producto, AppDbContext db, ClienteOpenFoodFacts fuente, CancellationToken cancelar)
    {
        var propuesta = await fuente.Consultar(producto.Codigo, cancelar);
        if (propuesta.Url == null) return propuesta;
        var rechazadas = await db.ImagenesRechazadas.AsNoTracking().Where(r => r.IdProducto == producto.Id).ToListAsync(cancelar);
        return ServicioImagenesTienda.EstaRechazada(propuesta.Url, rechazadas) ? PropuestaImagen.Ninguna : propuesta;
    }

    private static async Task DescartarPropuesta(AppDbContext db, Producto producto, string? url)
    {
        if (string.IsNullOrEmpty(url)) return;
        var rechazadas = await db.ImagenesRechazadas.Where(r => r.IdProducto == producto.Id).ToListAsync();
        var nueva = ServicioImagenesTienda.Rechazar(producto, url, rechazadas);
        if (nueva != null) db.ImagenesRechazadas.Add(nueva);
    }

    private static async Task<string> DescargarYGuardar(string url, ClienteOpenFoodFacts fuente, AlmacenImagenes almacen, CancellationToken cancelar)
    {
        var preparada = ProcesadorImagen.Preparar(await fuente.Descargar(url, cancelar));
        return await almacen.GuardarDeApi(url, preparada.Datos, cancelar);
    }

    /// <summary>Guarda la fila. Si falla, no queda un archivo propio suelto; el de la API es caché y se queda.</summary>
    private static async Task Guardar(AppDbContext db, AlmacenImagenes almacen, ImagenProducto nueva, CancellationToken cancelar)
    {
        try
        {
            var imagenes = await db.ImagenesProducto.Where(i => i.IdProducto == nueva.IdProducto).ToListAsync(cancelar);
            var anterior = ServicioImagenesTienda.Colocar(imagenes, nueva);
            await using var transaccion = await db.Database.BeginTransactionAsync(cancelar);
            if (anterior != null)
            {
                db.ImagenesProducto.Remove(anterior);
                await db.SaveChangesAsync(cancelar);
            }
            db.ImagenesProducto.Add(nueva);
            await db.SaveChangesAsync(cancelar);
            await transaccion.CommitAsync(cancelar);
            if (anterior != null) await LiberarPropia(db, almacen, anterior);
        }
        catch
        {
            await LiberarPropia(db, almacen, nueva);
            throw;
        }
    }

    /// <summary>Una imagen propia idéntica puede estar en otro producto del local: el archivo se borra solo si nadie más lo usa.</summary>
    private static async Task LiberarPropia(AppDbContext db, AlmacenImagenes almacen, ImagenProducto imagen)
    {
        if (imagen.Origen != ServicioImagenesTienda.OrigenPropia) return;
        var enUso = await db.ImagenesProducto.AsNoTracking()
            .AnyAsync(i => i.Archivo == imagen.Archivo && i.Origen == ServicioImagenesTienda.OrigenPropia && i.Id != imagen.Id);
        if (!enUso) almacen.BorrarPropia(imagen);
    }

    private static string EnlaceDe(Producto producto) => $"https://world.openfoodfacts.org/product/{producto.Codigo}";

    private static string? DataUrl(AlmacenImagenes almacen, ImagenProducto imagen)
    {
        var ruta = almacen.Ruta(imagen);
        return ruta == null ? null : $"data:{imagen.TipoContenido};base64,{Convert.ToBase64String(File.ReadAllBytes(ruta))}";
    }
}
