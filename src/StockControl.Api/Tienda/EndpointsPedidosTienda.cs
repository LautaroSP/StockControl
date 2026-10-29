using Microsoft.EntityFrameworkCore;
using StockControl.Api.Avisos;
using StockControl.Api.Contratos;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

/// <summary>Carrito y pedido del comprador, sin cuenta. El código del link es la única llave del pedido.</summary>
public static class EndpointsPedidosTienda
{
    public static void MapPedidosTienda(this WebApplication app)
    {
        app.MapPost("/tienda/{alias}/carrito", Cotizar).RequireRateLimiting("tienda");
        app.MapPost("/tienda/{alias}/pedidos", Crear).RequireRateLimiting("tienda-pedidos");
        app.MapGet("/tienda/{alias}/pedidos/{codigo}", Consultar).RequireRateLimiting("tienda");
        app.MapPost("/tienda/{alias}/pedidos/{codigo}/cancelar", Cancelar).RequireRateLimiting("tienda-pedidos");
    }

    private static async Task<IResult> Cotizar(string alias, CarritoTiendaPedido carrito, AppDbContext db, SesionActual sesion)
    {
        var local = await LocalesTienda.PorAlias(db, alias);
        if (local == null || ServicioTienda.Vista(local, DateTimeOffset.UtcNow) != ServicioTienda.VistaCatalogo)
            return Results.NotFound();
        sesion.IdLocal = local.IdLocal;
        try
        {
            var items = Items(carrito.Items);
            var cotizacion = ServicioPedidosTienda.Cotizar(
                local.IdLocal, items, await Productos(db, items), await ReservasTienda.DelLocal(db, local.IdLocal), await StockRigido(db));
            return Results.Ok(new
            {
                lineas = cotizacion.Lineas.Select(l => new
                {
                    idProducto = l.IdProducto, nombre = l.Nombre, cantidad = l.Cantidad,
                    precioUnitario = l.PrecioUnitario, subTotal = l.SubTotal, maximo = l.Maximo
                }),
                noDisponibles = cotizacion.NoDisponibles,
                total = cotizacion.Total
            });
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> Crear(string alias, PedidoTiendaPedido pedido, AppDbContext db, SesionActual sesion, ColaPush cola)
    {
        if (ServicioContacto.EsCebo(pedido.Sitio)) return Results.Ok();
        var local = await LocalesTienda.PorAlias(db, alias);
        if (local == null || ServicioTienda.Vista(local, DateTimeOffset.UtcNow) == null)
            return Results.NotFound();
        sesion.IdLocal = local.IdLocal;
        try
        {
            ServicioPedidosTienda.ExigirAbierta(local, DateTimeOffset.UtcNow);
            var items = Items(pedido.Items);
            await using var tx = await db.Database.BeginTransactionAsync();
            await ReservasTienda.Bloquear(db, local.IdLocal);
            var numero = (await db.PedidosTienda.MaxAsync(p => (int?)p.Numero) ?? 0) + 1;
            var creado = ServicioPedidosTienda.Crear(
                local, DateTimeOffset.UtcNow, numero,
                new DatosComprador(pedido.Nombre, pedido.Telefono, pedido.Email),
                items, await Productos(db, items), await ReservasTienda.DelLocal(db, local.IdLocal), await StockRigido(db));
            db.PedidosTienda.Add(creado.Pedido);
            await db.SaveChangesAsync();
            var push = await AvisosPedidos.Agregar(db, creado.Pedido, ServicioAvisos.PedidoNuevo);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            cola.Encolar(push);
            return Results.Ok(new { numero = creado.Pedido.Numero, estado = creado.Pedido.Estado, codigo = creado.Codigo });
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> Consultar(string alias, string codigo, AppDbContext db, SesionActual sesion)
    {
        var (local, pedido) = await PedidoDelCodigo(alias, codigo, db, sesion);
        if (local == null || pedido == null) return Results.NotFound();
        return Results.Ok(new
        {
            numero = pedido.Numero,
            estado = pedido.Estado,
            puedeCancelar = ServicioPedidosTienda.PuedeCancelar(pedido),
            creado = pedido.Creado,
            nombre = pedido.Nombre,
            subtotal = pedido.Subtotal,
            total = pedido.Total,
            lineas = pedido.Lineas.OrderBy(l => l.Id).Select(l => new
            {
                nombre = l.Nombre, cantidad = l.Cantidad, precioUnitario = l.PrecioUnitario, subTotal = l.SubTotal
            }),
            local = new { nombre = local.Nombre, direccion = local.Direccion, horario = local.Horario, whatsApp = local.WhatsApp }
        });
    }

    private static async Task<IResult> Cancelar(string alias, string codigo, AppDbContext db, SesionActual sesion, ColaPush cola)
    {
        var (local, pedido) = await PedidoDelCodigo(alias, codigo, db, sesion, rastrear: true);
        if (local == null || pedido == null) return Results.NotFound();
        try
        {
            ServicioPedidosTienda.Cancelar(pedido, DateTimeOffset.UtcNow);
            var push = await AvisosPedidos.Agregar(db, pedido, ServicioAvisos.PedidoCancelado);
            await db.SaveChangesAsync();
            cola.Encolar(push);
            return Results.Ok(new { estado = pedido.Estado });
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Results.BadRequest("El local ya está preparando el pedido. Escribile para cancelarlo.");
        }
    }

    /// <summary>Alias o código equivocados dan el mismo 404. El pedido se ve aunque la tienda esté pausada o cerrada.</summary>
    private static async Task<(Local?, PedidoTienda?)> PedidoDelCodigo(
        string alias, string codigo, AppDbContext db, SesionActual sesion, bool rastrear = false)
    {
        var local = await LocalesTienda.PorAlias(db, alias);
        if (local == null) return (null, null);
        sesion.IdLocal = local.IdLocal;
        var hash = ServicioPedidosTienda.HashDeCodigo(codigo);
        var consulta = db.PedidosTienda.Include(p => p.Lineas).Where(p => p.HashCodigo == hash);
        var pedido = await (rastrear ? consulta : consulta.AsNoTracking()).FirstOrDefaultAsync();
        return (local, pedido);
    }

    private static List<ItemPedidoTienda> Items(List<ItemPedidoTiendaPedido>? items)
    {
        var lista = items ?? [];
        if (lista.Count > ServicioPedidosTienda.MaximoLineas)
            throw new ErrorNegocio($"Un pedido admite hasta {ServicioPedidosTienda.MaximoLineas} productos distintos.");
        return lista.Select(i => new ItemPedidoTienda(i.IdProducto, i.Cantidad)).ToList();
    }

    private static Task<List<Producto>> Productos(AppDbContext db, List<ItemPedidoTienda> items)
    {
        var ids = items.Select(i => i.IdProducto).Distinct().ToList();
        return db.Productos.AsNoTracking().Where(p => ids.Contains(p.Id)).ToListAsync();
    }

    private static async Task<bool> StockRigido(AppDbContext db) =>
        (await db.Configuracion.AsNoTracking().FirstOrDefaultAsync(c => c.Clave == "StockRigido"))?.Valor == "1";
}
