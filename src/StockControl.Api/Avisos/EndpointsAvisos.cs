using Microsoft.EntityFrameworkCore;
using StockControl.Api.Contratos;
using StockControl.Api.Datos;
using StockControl.Api.Tienda;
using StockControl.Dominio;

namespace StockControl.Api.Avisos;

    /// <summary>Campanita, preferencias y suscripción Web Push. Todo es del usuario en el local abierto. Un dispositivo avisa solo a quien lo activó último.</summary>
public static class EndpointsAvisos
{
    public static void MapAvisos(this WebApplication app)
    {
        var grupo = app.MapGroup("/avisos").RequireAuthorization().RequireRateLimiting("api");
        grupo.MapGet("/contador", Contador);
        grupo.MapGet("", Listar);
        grupo.MapPost("/{id:int}/leido", MarcarLeido);
        grupo.MapPost("/leidos", MarcarTodos);
        grupo.MapGet("/preferencias", Preferencias);
        grupo.MapPut("/preferencias", GuardarPreferencias);
        grupo.MapPost("/push", Suscribir);
        grupo.MapPost("/push/baja", Desuscribir);
    }

    private static IQueryable<Aviso> DelUsuario(AppDbContext db, SesionActual sesion) =>
        db.Avisos.Where(a => a.IdUsuario == sesion.IdUsuario);

    private static async Task<IResult> Contador(AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal == null || sesion.IdUsuario == null) return Results.BadRequest("Elegí un local.");
        if (!await PermisoPedidosTienda.Tiene(db, sesion))
            return Results.Ok(new { puedeVerPedidos = false, noLeidos = 0, ultimo = 0, sonido = false });
        var noLeidos = await DelUsuario(db, sesion).CountAsync(a => a.Leido == null);
        var ultimo = await DelUsuario(db, sesion).MaxAsync(a => (int?)a.Id) ?? 0;
        var preferencia = await Preferencia(db, sesion);
        return Results.Ok(new { puedeVerPedidos = true, noLeidos, ultimo, sonido = preferencia.Sonido });
    }

    private static async Task<IResult> Listar(AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal == null || sesion.IdUsuario == null) return Results.BadRequest("Elegí un local.");
        if (!await PermisoPedidosTienda.Tiene(db, sesion)) return Results.Forbid();
        var items = await DelUsuario(db, sesion).AsNoTracking()
            .OrderByDescending(a => a.Id)
            .Take(ServicioAvisos.TopeLista)
            .Select(a => new { a.Id, a.Tipo, a.Texto, a.IdPedidoTienda, a.Creado, leido = a.Leido != null })
            .ToListAsync();
        return Results.Ok(items);
    }

    private static async Task<IResult> MarcarLeido(int id, AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal == null || sesion.IdUsuario == null) return Results.BadRequest("Elegí un local.");
        var aviso = await DelUsuario(db, sesion).FirstOrDefaultAsync(a => a.Id == id);
        if (aviso == null) return Results.NotFound();
        ServicioAvisos.MarcarLeido(aviso, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> MarcarTodos(AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal == null || sesion.IdUsuario == null) return Results.BadRequest("Elegí un local.");
        var ahora = DateTimeOffset.UtcNow;
        await DelUsuario(db, sesion).Where(a => a.Leido == null)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Leido, ahora));
        return Results.NoContent();
    }

    private static async Task<IResult> Preferencias(AppDbContext db, SesionActual sesion, IConfiguration config)
    {
        if (sesion.IdLocal == null || sesion.IdUsuario == null) return Results.BadRequest("Elegí un local.");
        var p = await Preferencia(db, sesion);
        return Results.Ok(new { p.Sonido, p.PushNuevos, p.PushCancelados, clavePush = ClavesVapid.Publica(config) });
    }

    private static async Task<IResult> GuardarPreferencias(PreferenciasAvisoPedido pedido, AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal is not int idLocal || sesion.IdUsuario is not int idUsuario) return Results.BadRequest("Elegí un local.");
        var p = await db.PreferenciasAviso.FirstOrDefaultAsync(x => x.IdUsuario == idUsuario);
        if (p == null)
        {
            p = new PreferenciaAviso { IdLocal = idLocal, IdUsuario = idUsuario };
            db.PreferenciasAviso.Add(p);
        }
        p.Sonido = pedido.Sonido;
        p.PushNuevos = pedido.PushNuevos;
        p.PushCancelados = pedido.PushCancelados;
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> Suscribir(SuscripcionPushPedido pedido, AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal is not int idLocal || sesion.IdUsuario is not int idUsuario) return Results.BadRequest("Elegí un local.");
        if (!await PermisoPedidosTienda.Tiene(db, sesion)) return Results.Forbid();
        try
        {
            var nueva = ServicioPush.Crear(idLocal, idUsuario, pedido.Endpoint, pedido.P256dh, pedido.Auth, DateTimeOffset.UtcNow);
            await db.SuscripcionesPush.IgnoreQueryFilters()
                .Where(s => s.Endpoint == nueva.Endpoint && s.IdUsuario != idUsuario)
                .ExecuteDeleteAsync();
            var propias = await db.SuscripcionesPush.Where(s => s.IdUsuario == idUsuario).OrderBy(s => s.Creado).ToListAsync();
            db.SuscripcionesPush.RemoveRange(propias.Where(s => s.Endpoint == nueva.Endpoint));
            db.SuscripcionesPush.RemoveRange(propias.Where(s => s.Endpoint != nueva.Endpoint).SkipLast(ServicioPush.MaximoPorUsuario - 1));
            await db.SaveChangesAsync();
            db.SuscripcionesPush.Add(nueva);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }
        catch (ErrorNegocio ex) { return Results.BadRequest(ex.Message); }
    }

    private static async Task<IResult> Desuscribir(SuscripcionPushPedido pedido, AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal == null || sesion.IdUsuario is not int idUsuario) return Results.BadRequest("Elegí un local.");
        await db.SuscripcionesPush.Where(s => s.IdUsuario == idUsuario && s.Endpoint == pedido.Endpoint).ExecuteDeleteAsync();
        return Results.NoContent();
    }

    private static async Task<PreferenciaAviso> Preferencia(AppDbContext db, SesionActual sesion) =>
        await db.PreferenciasAviso.AsNoTracking().FirstOrDefaultAsync(p => p.IdUsuario == sesion.IdUsuario) ?? new PreferenciaAviso();
}
