using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using StockControl.Api.Datos;

namespace StockControl.Api.Auth;

public class MiddlewareSesion
{
    private readonly RequestDelegate _next;

    public MiddlewareSesion(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext http, SesionActual sesion)
    {
        if (http.User.Identity?.IsAuthenticated == true)
        {
            var sub = http.User.FindFirstValue("sub")
                      ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(sub, out var id))
                sesion.IdUsuario = id;
            sesion.Rol = http.User.FindFirstValue("rol");
            if (int.TryParse(http.User.FindFirstValue("localId"), out var local))
                sesion.IdLocal = local;
            if (int.TryParse(http.User.FindFirstValue("nroCaja"), out var caja))
                sesion.NroCaja = caja;
        }

        await _next(http);
    }
}
