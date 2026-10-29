using Microsoft.EntityFrameworkCore;
using StockControl.Api.Datos;
using StockControl.Api.Tienda;
using StockControl.Dominio;

namespace StockControl.Api.Avisos;

/// <summary>Agrega los avisos del pedido al contexto. El push se encola después de confirmar la transacción.</summary>
public static class AvisosPedidos
{
    public static async Task<MensajePush> Agregar(AppDbContext db, PedidoTienda pedido, string tipo)
    {
        var vinculos = await db.UsuarioLocales.AsNoTracking().Where(v => v.IdLocal == pedido.IdLocal).ToListAsync();
        var ids = vinculos.Select(v => v.IdUsuario).ToList();
        var usuarios = await db.Usuarios.AsNoTracking().Where(u => ids.Contains(u.Id)).ToListAsync();
        var preferencias = await db.PreferenciasAviso.IgnoreQueryFilters().AsNoTracking()
            .Where(p => p.IdLocal == pedido.IdLocal).ToListAsync();
        var empleado = await PermisoPedidosTienda.EmpleadoHabilitado(db, pedido.IdLocal);

        var avisos = ServicioAvisos.DePedido(tipo, pedido, vinculos, usuarios, preferencias, empleado, DateTimeOffset.UtcNow);
        db.Avisos.AddRange(avisos.Avisos);
        return new MensajePush(pedido.IdLocal, avisos.IdsUsuarioPush, avisos.Titulo, avisos.Texto, $"/mi-tienda/pedidos?id={pedido.Id}");
    }
}
