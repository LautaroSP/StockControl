using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

public static class PermisoPedidosTienda
{
    public const string Clave = "EmpleadoVePedidosTienda";

    public static async Task<bool> EmpleadoHabilitado(AppDbContext db, int idLocal) =>
        (await db.Configuracion.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdLocal == idLocal && c.Clave == Clave))?.Valor == "1";

    public static async Task<bool> Tiene(AppDbContext db, SesionActual sesion) =>
        sesion.IdLocal is int idLocal
        && Permisos.PuedeGestionarPedidosTienda(sesion.Rol ?? "", await EmpleadoHabilitado(db, idLocal));
}
