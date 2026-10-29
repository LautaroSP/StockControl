using Microsoft.EntityFrameworkCore;
using StockControl.Api.Contratos;
using StockControl.Api.Datos;
using StockControl.Api.Pagos;
using StockControl.Dominio;

namespace StockControl.Api.Tienda;

/// <summary>Panel de pedidos de la tienda: dueño, socio y el empleado habilitado.</summary>
public static class EndpointsPanelPedidos
{
    private const int TamanoPagina = 50;

    public static void MapPanelPedidos(this WebApplication app)
    {
        var grupo = app.MapGroup("/pedidos-tienda").RequireAuthorization().RequireRateLimiting("api");
        grupo.MapGet("", Listar);
        grupo.MapGet("/{id:int}", Detalle);
        grupo.MapPost("/{id:int}/estado", Avanzar);
        grupo.MapPost("/{id:int}/entregar", Entregar);
        grupo.MapPost("/{id:int}/cancelar", Cancelar);
    }

    private static async Task<IResult> Listar(string? estado, AppDbContext db, SesionActual sesion, int pagina = 1)
    {
        if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
        if (!await PermisoPedidosTienda.Tiene(db, sesion)) return Results.Forbid();
        pagina = Math.Max(1, pagina);
        var consulta = Filtrar(db.PedidosTienda.AsNoTracking(), estado);
        var total = await consulta.CountAsync();
        var items = await consulta.OrderByDescending(p => p.Numero)
            .Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina)
            .Select(p => new { p.Id, p.Numero, p.Estado, p.Creado, p.Nombre, p.Total, productos = p.Lineas.Count })
            .ToListAsync();
        return Results.Ok(new { total, pagina, tamano = TamanoPagina, items });
    }

    private static async Task<IResult> Detalle(int id, AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
        if (!await PermisoPedidosTienda.Tiene(db, sesion)) return Results.Forbid();
        var pedido = await db.PedidosTienda.AsNoTracking().Include(p => p.Lineas).FirstOrDefaultAsync(p => p.Id == id);
        return pedido == null ? Results.NotFound() : Results.Ok(Vista(pedido));
    }

    private static async Task<IResult> Avanzar(int id, EstadoPedidoTiendaPedido pedido, AppDbContext db, SesionActual sesion) =>
        await Cambiar(id, db, sesion, p => ServicioPedidosTienda.Avanzar(p, pedido.Estado, DateTimeOffset.UtcNow));

    private static async Task<IResult> Cancelar(int id, AppDbContext db, SesionActual sesion) =>
        await Cambiar(id, db, sesion, p => ServicioPedidosTienda.CancelarDelLocal(p, DateTimeOffset.UtcNow));

    private static async Task<IResult> Cambiar(int id, AppDbContext db, SesionActual sesion, Action<PedidoTienda> cambio)
    {
        if (sesion.IdLocal == null) return Results.BadRequest("Elegí un local.");
        if (!await PermisoPedidosTienda.Tiene(db, sesion)) return Results.Forbid();
        var pedido = await db.PedidosTienda.Include(p => p.Lineas).FirstOrDefaultAsync(p => p.Id == id);
        if (pedido == null) return Results.NotFound();
        try
        {
            cambio(pedido);
            await db.SaveChangesAsync();
            return Results.Ok(Vista(pedido));
        }
        catch (ErrorNegocio ex) { return Results.BadRequest(ex.Message); }
        catch (DbUpdateConcurrencyException) { return Results.Conflict("El pedido cambió mientras lo editabas. Volvé a abrirlo."); }
    }

    /// <summary>La venta entra en el puesto de caja de quien entrega, con el medio que eligió.</summary>
    private static async Task<IResult> Entregar(int id, EntregarPedidoTiendaPedido pedido, AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal is not int idLocal) return Results.BadRequest("Elegí un local.");
        if (sesion.NroCaja is not int nroCaja) return Results.BadRequest("Elegí una caja.");
        if (!await PermisoPedidosTienda.Tiene(db, sesion)) return Results.Forbid();
        var local = await db.Locales.AsNoTracking().FirstAsync(l => l.IdLocal == idLocal);
        if (!ServicioAbono.Estado(local, DateTimeOffset.UtcNow).PuedeOperarCaja)
            return Results.Json("El local está vencido. Pagá el abono para seguir cobrando.", statusCode: StatusCodes.Status402PaymentRequired);
        var medio = await db.MetodosPago.AsNoTracking().FirstOrDefaultAsync(m => m.Id == pedido.IdMetodoPago && m.Activo);
        if (medio == null) return Results.BadRequest("El medio de pago no existe o está inactivo.");
        if (IntegracionPago.EsMercadoPago(medio.Integracion))
            return Results.BadRequest("Un pedido de la tienda se cobra con un medio que no espera a Mercado Pago.");
        try
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            await ReservasTienda.Bloquear(db, idLocal);
            var entregado = await db.PedidosTienda.Include(p => p.Lineas).FirstOrDefaultAsync(p => p.Id == id);
            if (entregado == null) return Results.NotFound();
            var ids = entregado.Lineas.Select(l => l.IdProducto).Distinct().ToList();
            var productos = await db.Productos.Where(p => ids.Contains(p.Id)).ToListAsync();
            var resultado = ServicioEntregaPedidoTienda.Entregar(entregado, productos, DateTimeOffset.UtcNow);
            var venta = await RegistrarVenta.Guardar(db, idLocal, sesion.IdUsuario, nroCaja, resultado,
                [new PagoSolicitado(medio.Id, resultado.Total)]);
            await db.SaveChangesAsync();
            entregado.IdInformeVenta = venta.IdInformeVenta;
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return Results.Ok(Vista(entregado));
        }
        catch (ErrorNegocio ex) { return Results.BadRequest(ex.Message); }
        catch (DbUpdateConcurrencyException) { return Results.Conflict("El pedido cambió mientras lo entregabas. Volvé a abrirlo."); }
    }

    private static IQueryable<PedidoTienda> Filtrar(IQueryable<PedidoTienda> consulta, string? estado) => estado switch
    {
        null or "" or "activos" => consulta.Where(p => ServicioPedidosTienda.Activos.Contains(p.Estado)),
        "todos" => consulta,
        _ => consulta.Where(p => p.Estado == estado)
    };

    private static object Vista(PedidoTienda p) => new
    {
        p.Id,
        p.Numero,
        p.Estado,
        p.Creado,
        p.Actualizado,
        p.Cancelado,
        p.Entregado,
        p.IdInformeVenta,
        p.Nombre,
        p.Telefono,
        telefonoVisible = ServicioAdmin.MostrarTelefono(p.Telefono),
        p.Email,
        p.Subtotal,
        p.Total,
        siguientes = ServicioPedidosTienda.Siguientes(p.Estado),
        lineas = p.Lineas.OrderBy(l => l.Id).Select(l => new { l.Id, l.Codigo, l.Nombre, l.Cantidad, l.PrecioUnitario, l.SubTotal })
    };
}
