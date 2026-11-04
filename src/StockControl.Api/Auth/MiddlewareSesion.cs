using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Auth;

public class MiddlewareSesion
{
    private readonly RequestDelegate _next;

    public MiddlewareSesion(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext http, SesionActual sesion, AppDbContext db)
    {
        if (http.User.Identity?.IsAuthenticated == true)
        {
            var sub = http.User.FindFirstValue("sub")
                      ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(sub, out var id))
                sesion.IdUsuario = id;
            sesion.Rol = http.User.FindFirstValue("rol");
            sesion.NombreUsuario = http.User.FindFirstValue("usuario");
            if (int.TryParse(http.User.FindFirstValue("localId"), out var local))
                sesion.IdLocal = local;
            if (int.TryParse(http.User.FindFirstValue("nroCaja"), out var caja))
                sesion.NroCaja = caja;
            int? versionToken = int.TryParse(http.User.FindFirstValue("sesion"), out var version) ? version : null;
            if (sesion.IdUsuario is int idUsuario)
            {
                var fila = await db.Usuarios.AsNoTracking()
                    .Where(u => u.Id == idUsuario)
                    .Select(u => new { u.Activo, u.VersionSesion })
                    .FirstOrDefaultAsync();
                if (fila is not { Activo: true } || !ServicioUsuarios.SesionVigente(versionToken, fila.VersionSesion))
                {
                    http.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }
            if (sesion.IdLocal is int idLocal)
            {
                var activoLocal = await db.Locales.AsNoTracking()
                    .Where(l => l.IdLocal == idLocal)
                    .Select(l => (bool?)l.Activo)
                    .FirstOrDefaultAsync();
                if (activoLocal == false && !RutaConLocalDesactivado(http.Request.Path))
                {
                    http.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await http.Response.WriteAsync("Ese local está desactivado.");
                    return;
                }
            }
        }

        await _next(http);
    }

    private static bool RutaConLocalDesactivado(PathString ruta)
    {
        var path = ruta.Value ?? "";
        if (path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase)) return true;
        if (path.StartsWith("/auth", StringComparison.OrdinalIgnoreCase)) return true;
        return path.StartsWith("/locales", StringComparison.OrdinalIgnoreCase)
            && !path.Contains("/actual", StringComparison.OrdinalIgnoreCase);
    }
}
