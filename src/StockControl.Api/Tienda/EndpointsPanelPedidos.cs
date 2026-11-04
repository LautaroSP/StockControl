using Microsoft.AspNetCore.DataProtection;
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
        grupo.MapPost("/{id:int}/reembolsar", Reembolsar);
        grupo.MapPost("/{id:int}/confirmar-transferencia", ConfirmarTransferencia);
        grupo.MapPost("/{id:int}/devolver-transferencia", DevolverTransferencia);
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
            .Select(p => new { p.Id, p.Numero, p.Estado, p.MedioPago, p.EstadoPago, p.Creado, p.Nombre, p.Total, p.Modalidad, productos = p.Lineas.Count })
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

    /// <summary>La plata de la transferencia la ven el dueño y el socio, no el empleado.</summary>
    private static async Task<IResult> ConfirmarTransferencia(int id, AppDbContext db, SesionActual sesion)
    {
        if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return Results.Forbid();
        if (sesion.IdLocal is not int idLocal) return Results.BadRequest("Elegí un local.");
        var vence = await db.Locales.Where(l => l.IdLocal == idLocal).Select(l => l.Vence).FirstAsync();
        var yaExiste = await db.GestionesTienda.AnyAsync(g => g.IdPedido == id);
        return await Cambiar(id, db, sesion, pedido =>
        {
            var ahora = DateTimeOffset.UtcNow;
            ServicioTransferenciaTienda.Confirmar(pedido, ahora);
            var deuda = ServicioGestionTienda.Nacer(pedido, vence, ahora, yaExiste);
            if (deuda != null) db.GestionesTienda.Add(deuda);
        });
    }

    private static async Task<IResult> DevolverTransferencia(int id, AppDbContext db, SesionActual sesion)
    {
        if (!Permisos.PuedeConfigurarLocal(sesion.Rol ?? "")) return Results.Forbid();
        var deuda = await db.GestionesTienda.FirstOrDefaultAsync(g => g.IdPedido == id);
        return await Cambiar(id, db, sesion, pedido =>
        {
            ServicioTransferenciaTienda.CancelarConfirmada(pedido, DateTimeOffset.UtcNow);
            ServicioGestionTienda.Anular(deuda);
        });
    }

    private static async Task<IResult> Reembolsar(
        int id, AppDbContext db, SesionActual sesion, ClienteMercadoPagoLocal mp, IDataProtectionProvider proteccion,
        IConfiguration config, ILoggerFactory logs)
    {
        if (sesion.IdLocal is not int idLocal) return Results.BadRequest("Elegí un local.");
        if (!await PermisoPedidosTienda.Tiene(db, sesion)) return Results.Forbid();
        try
        {
            var token = await PagosPedidoTienda.Token(db, mp, proteccion.CreateProtector(CobrosMercadoPagoApp.Proposito), config, idLocal);
            await using var tx = await db.Database.BeginTransactionAsync();
            await ReservasTienda.Bloquear(db, idLocal);
            var pedido = await db.PedidosTienda.Include(p => p.Lineas).FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null) return Results.NotFound();
            await PagosPedidoTienda.CancelarConReembolso(db, mp, token, pedido);
            await tx.CommitAsync();
            return Results.Ok(Vista(pedido));
        }
        catch (ErrorNegocio ex) { return Results.BadRequest(ex.Message); }
        catch (DbUpdateConcurrencyException) { return Results.Conflict("El pedido cambió mientras lo cancelabas. Volvé a abrirlo."); }
        catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
        {
            logs.CreateLogger("MercadoPago").LogError(ex, "No se pudo reembolsar el pedido {IdPedido}", id);
            return Results.Json("Mercado Pago no hizo la devolución. El pedido sigue pagado: probá de nuevo.", statusCode: StatusCodes.Status502BadGateway);
        }
    }

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

    /// <summary>
    /// La venta entra en el puesto de caja de quien entrega. Pagado por Mercado Pago, con el medio Mercado Pago del local;
    /// sin pagar o por transferencia, con el medio que eligió.
    /// </summary>
    private static async Task<IResult> Entregar(int id, EntregarPedidoTiendaPedido pedido, AppDbContext db, SesionActual sesion)
    {
        if (sesion.IdLocal is not int idLocal) return Results.BadRequest("Elegí un local.");
        if (sesion.NroCaja is not int nroCaja) return Results.BadRequest("Elegí una caja.");
        if (!await PermisoPedidosTienda.Tiene(db, sesion)) return Results.Forbid();
        var local = await db.Locales.AsNoTracking().FirstAsync(l => l.IdLocal == idLocal);
        if (!ServicioAbono.Estado(local, DateTimeOffset.UtcNow).PuedeOperarCaja)
            return Results.Json("El local está vencido. Pagá el abono para seguir cobrando.", statusCode: StatusCodes.Status402PaymentRequired);
        try
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            await ReservasTienda.Bloquear(db, idLocal);
            var entregado = await db.PedidosTienda.Include(p => p.Lineas).FirstOrDefaultAsync(p => p.Id == id);
            if (entregado == null) return Results.NotFound();
            var medio = await MedioDeEntrega(db, entregado, pedido.IdMetodoPago);
            var ids = entregado.Lineas.Select(l => l.IdProducto).Distinct().ToList();
            var productos = await db.Productos.Where(p => ids.Contains(p.Id)).ToListAsync();
            var ahora = DateTimeOffset.UtcNow;
            var resultado = ServicioEntregaPedidoTienda.Entregar(entregado, productos, ahora);
            var yaExiste = await db.GestionesTienda.AnyAsync(g => g.IdPedido == entregado.Id);
            var deuda = ServicioGestionTienda.Nacer(entregado, local.Vence, ahora, yaExiste);
            if (deuda != null) db.GestionesTienda.Add(deuda);
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

    private static async Task<MetodoPago> MedioDeEntrega(AppDbContext db, PedidoTienda pedido, int idMetodoPago)
    {
        var activos = db.MetodosPago.AsNoTracking().Where(m => m.Activo);
        if (pedido.EstadoPago == ServicioPagoPedidoTienda.Pagado && pedido.MedioPago == ServicioPagoPedidoTienda.MercadoPago)
            return await activos.Where(m => m.Integracion != IntegracionPago.Manual).OrderBy(m => m.Id).FirstOrDefaultAsync()
                ?? throw new ErrorNegocio("Activá un medio de pago con Mercado Pago en Medios de pago para registrar la venta.");
        var medio = await activos.FirstOrDefaultAsync(m => m.Id == idMetodoPago)
            ?? throw new ErrorNegocio("El medio de pago no existe o está inactivo.");
        if (IntegracionPago.EsMercadoPago(medio.Integracion))
            throw new ErrorNegocio("Un pedido de la tienda se cobra con un medio que no espera a Mercado Pago.");
        return medio;
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
        p.Envio,
        p.Total,
        p.Modalidad,
        p.DireccionEnvio,
        p.DistanciaKm,
        p.OrigenDistancia,
        p.MedioPago,
        p.EstadoPago,
        p.Gestion,
        p.GestionACargoComprador,
        p.Pagado,
        p.VencePago,
        esperaPago = ServicioPagoPedidoTienda.EsperaPago(p),
        siguientes = ServicioPagoPedidoTienda.Siguientes(p),
        lineas = p.Lineas.OrderBy(l => l.Id).Select(l => new { l.Id, l.Codigo, l.Nombre, l.Cantidad, l.PrecioUnitario, l.SubTotal })
    };
}
