using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using StockControl.Api.Avisos;
using StockControl.Api.Contratos;
using StockControl.Api.Datos;
using StockControl.Api.Pagos;
using StockControl.Dominio;
using System.Text.Json;

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
        app.MapGet("/tienda/{alias}/ubicacion", BuscarUbicacion).RequireRateLimiting("nominatim");
    }

    private static async Task<IResult> Cotizar(string alias, CarritoTiendaPedido carrito, AppDbContext db, AlcanceDatos alcance, SesionActual sesion, ClienteOpenRouteService rutas, CancellationToken cancelar)
    {
        var local = await LocalesTienda.PorAlias(db, alcance, sesion, alias);
        if (local == null) return Results.NotFound();
        var ahora = DateTimeOffset.UtcNow;
        if (ServicioTienda.Vista(local, ahora, await ConsultaGestionTienda.Vencida(db, local.IdLocal, ahora)) != ServicioTienda.VistaCatalogo)
            return Results.NotFound();
        sesion.IdLocal = local.IdLocal;
        try
        {
            var items = Items(carrito.Items);
            var cotizacion = ServicioPedidosTienda.Cotizar(
                local.IdLocal, items, await Productos(db, items), await ReservasTienda.DelLocal(db, local.IdLocal), await StockRigido(db));
            var desglose = ServicioGestionTienda.Desglose(local, cotizacion.Total);
            var pagoPrevio = new { gestion = desglose.ACargoComprador ? desglose.Gestion : 0, total = desglose.Total };
            var km = await Kilometros(rutas, local, carrito.Latitud, carrito.Longitud, cancelar);
            var envio = ServicioEnvioTienda.Cotizar(local, cotizacion.Total, carrito.Latitud, carrito.Longitud, km);
            return Results.Ok(new
            {
                lineas = cotizacion.Lineas.Select(l => new
                {
                    idProducto = l.IdProducto, nombre = l.Nombre, cantidad = l.Cantidad,
                    precioUnitario = l.PrecioUnitario, subTotal = l.SubTotal, maximo = l.Maximo
                }),
                noDisponibles = cotizacion.NoDisponibles,
                total = cotizacion.Total,
                envio = new
                {
                    ofrece = ServicioEnvioTienda.Ofrece(local),
                    disponible = envio.Disponible,
                    distanciaKm = envio.DistanciaKm,
                    importe = envio.Importe,
                    origen = envio.Origen,
                    motivo = envio.Motivo
                },
                mercadoPago = await PagosPedidoTienda.AceptaMercadoPago(db, local.IdLocal) ? pagoPrevio : null,
                transferencia = ServicioTransferenciaTienda.Acepta(local) ? pagoPrevio : null
            });
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    /// <summary>Con Mercado Pago el local recibe el aviso recién al acreditarse el pago. Con transferencia, ya: la tiene que confirmar.</summary>
    private static async Task<IResult> Crear(
        string alias, PedidoTiendaPedido pedido, AppDbContext db, AlcanceDatos alcance, SesionActual sesion, ColaPush cola,
        ClienteMercadoPagoLocal mp, ClienteOpenRouteService rutas, IDataProtectionProvider proteccion, IConfiguration config, ILoggerFactory logs, CancellationToken cancelar)
    {
        if (ServicioContacto.EsCebo(pedido.Sitio)) return Results.Ok();
        var local = await LocalesTienda.PorAlias(db, alcance, sesion, alias);
        var ahoraPedido = DateTimeOffset.UtcNow;
        var gestionVencida = local != null && await ConsultaGestionTienda.Vencida(db, local.IdLocal, ahoraPedido);
        if (local == null || ServicioTienda.Vista(local, ahoraPedido, gestionVencida) == null)
            return Results.NotFound();
        sesion.IdLocal = local.IdLocal;
        PedidoCreado creado;
        try
        {
            ServicioPedidosTienda.ExigirAbierta(local, ahoraPedido, gestionVencida);
            var medio = ServicioPagoPedidoTienda.NormalizarMedio(
                pedido.MedioPago, await PagosPedidoTienda.AceptaMercadoPago(db, local.IdLocal), ServicioTransferenciaTienda.Acepta(local));
            var items = Items(pedido.Items);
            await using var tx = await db.Database.BeginTransactionAsync();
            await ReservasTienda.Bloquear(db, local.IdLocal);
            var numero = (await db.PedidosTienda.MaxAsync(p => (int?)p.Numero) ?? 0) + 1;
            var km = pedido.Modalidad == ServicioEnvioTienda.Envio
                ? await Kilometros(rutas, local, pedido.Latitud, pedido.Longitud, cancelar)
                : null;
            creado = ServicioPedidosTienda.Crear(
                local, ahoraPedido, numero,
                new DatosComprador(pedido.Nombre, pedido.Telefono, pedido.Email),
                items, await Productos(db, items), await ReservasTienda.DelLocal(db, local.IdLocal), await StockRigido(db), medio, gestionVencida,
                new DatosEnvio(pedido.Modalidad, pedido.DireccionEnvio, pedido.Latitud, pedido.Longitud, km));
            db.PedidosTienda.Add(creado.Pedido);
            await db.SaveChangesAsync();
            MensajePush? push = null;
            if (!ServicioPagoPedidoTienda.EsperaMercadoPago(creado.Pedido))
            {
                push = await AvisosPedidos.Agregar(db, creado.Pedido, ServicioAvisos.PedidoNuevo);
                await db.SaveChangesAsync();
            }
            await tx.CommitAsync();
            if (push != null) cola.Encolar(push);
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }

        if (ServicioPagoPedidoTienda.EsperaMercadoPago(creado.Pedido))
        {
            try
            {
                await PagosPedidoTienda.IniciarPago(db, mp, proteccion.CreateProtector(CobrosMercadoPagoApp.Proposito), config, local, creado.Pedido, creado.Codigo);
            }
            catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException or TaskCanceledException)
            {
                logs.CreateLogger("MercadoPago").LogError(ex, "No se pudo armar el pago del pedido {IdPedido}", creado.Pedido.Id);
                return Results.Json("Mercado Pago no respondió. El pedido no se guardó: probá de nuevo o elegí coordinar con el local.",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        }
        return Results.Ok(new
        {
            numero = creado.Pedido.Numero,
            estado = creado.Pedido.Estado,
            codigo = creado.Codigo,
            urlPago = creado.Pedido.UrlPagoMp.Length > 0 ? creado.Pedido.UrlPagoMp : null
        });
    }

    private static async Task<IResult> Consultar(string alias, string codigo, AppDbContext db, AlcanceDatos alcance, SesionActual sesion)
    {
        var (local, pedido) = await PedidoDelCodigo(alias, codigo, db, alcance, sesion);
        if (local == null || pedido == null) return Results.NotFound();
        return Results.Ok(new
        {
            numero = pedido.Numero,
            estado = pedido.Estado,
            puedeCancelar = ServicioPedidosTienda.PuedeCancelar(pedido),
            creado = pedido.Creado,
            nombre = pedido.Nombre,
            subtotal = pedido.Subtotal,
            gestion = pedido.GestionACargoComprador ? pedido.Gestion : 0,
            envio = pedido.Envio,
            total = pedido.Total,
            modalidad = pedido.Modalidad,
            direccionEnvio = pedido.DireccionEnvio,
            distanciaKm = pedido.DistanciaKm,
            origenDistancia = pedido.OrigenDistancia,
            medioPago = pedido.MedioPago,
            estadoPago = pedido.EstadoPago,
            urlPago = ServicioPagoPedidoTienda.EsperaMercadoPago(pedido) && pedido.Estado == ServicioPedidosTienda.Nuevo && pedido.UrlPagoMp.Length > 0
                ? pedido.UrlPagoMp
                : null,
            venceElPago = ServicioPagoPedidoTienda.EsperaPago(pedido) ? pedido.VencePago : null,
            transferencia = ServicioTransferenciaTienda.EsperaConfirmacion(pedido) && pedido.Estado == ServicioPedidosTienda.Nuevo
                ? new { cbu = local.CbuTienda, alias = local.AliasCbuTienda, titular = local.TitularTienda }
                : null,
            lineas = pedido.Lineas.OrderBy(l => l.Id).Select(l => new
            {
                nombre = l.Nombre, cantidad = l.Cantidad, precioUnitario = l.PrecioUnitario, subTotal = l.SubTotal
            }),
            local = new { nombre = local.Nombre, direccion = local.Direccion, horario = local.Horario, whatsApp = local.WhatsApp }
        });
    }

    private static async Task<IResult> Cancelar(string alias, string codigo, AppDbContext db, AlcanceDatos alcance, SesionActual sesion, ColaPush cola)
    {
        var (local, pedido) = await PedidoDelCodigo(alias, codigo, db, alcance, sesion, rastrear: true);
        if (local == null || pedido == null) return Results.NotFound();
        try
        {
            var avisado = !ServicioPagoPedidoTienda.EsperaMercadoPago(pedido);
            ServicioPedidosTienda.Cancelar(pedido, DateTimeOffset.UtcNow);
            var push = avisado ? await AvisosPedidos.Agregar(db, pedido, ServicioAvisos.PedidoCancelado) : null;
            await db.SaveChangesAsync();
            if (push != null) cola.Encolar(push);
            return Results.Ok(new { estado = pedido.Estado });
        }
        catch (ErrorNegocio ex)
        {
            return Results.BadRequest(ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Results.BadRequest("El pedido cambió recién (el local lo está preparando o se acreditó el pago). Volvé a abrirlo.");
        }
    }

    /// <summary>Alias o código equivocados dan el mismo 404. El pedido se ve aunque la tienda esté pausada o cerrada.</summary>
    private static async Task<(Local?, PedidoTienda?)> PedidoDelCodigo(
        string alias, string codigo, AppDbContext db, AlcanceDatos alcance, SesionActual sesion, bool rastrear = false)
    {
        var local = await LocalesTienda.PorAlias(db, alcance, sesion, alias);
        if (local == null) return (null, null);
        sesion.IdLocal = local.IdLocal;
        var hash = ServicioPedidosTienda.HashDeCodigo(codigo);
        var consulta = db.PedidosTienda.Include(p => p.Lineas).Where(p => p.HashCodigo == hash);
        var pedido = await (rastrear ? consulta : consulta.AsNoTracking()).FirstOrDefaultAsync();
        return (local, pedido);
    }

    private static async Task<IResult> BuscarUbicacion(string alias, string? q, AppDbContext db, AlcanceDatos alcance, SesionActual sesion, ClienteNominatim nominatim, ILoggerFactory logs, CancellationToken cancelar)
    {
        var local = await LocalesTienda.PorAlias(db, alcance, sesion, alias);
        var ahora = DateTimeOffset.UtcNow;
        if (local == null || ServicioTienda.Vista(local, ahora, await ConsultaGestionTienda.Vencida(db, local.IdLocal, ahora)) != ServicioTienda.VistaCatalogo)
            return Results.NotFound();
        sesion.IdLocal = local.IdLocal;
        var direccion = (q ?? "").Trim();
        if (direccion.Length < 4 || direccion.Length > ServicioTienda.LargoMaximoDato)
            return Results.BadRequest("Escribí la dirección con calle, número y localidad.");
        try
        {
            return Results.Ok(await nominatim.Buscar(direccion, cancelar));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logs.CreateLogger("Nominatim").LogWarning(ex, "Falló la búsqueda de dirección de un comprador");
            return Results.Problem("No se pudo buscar la dirección. Probá de nuevo.", statusCode: StatusCodes.Status502BadGateway);
        }
    }

    /// <summary>Null si no corresponde pedir el recorrido: el dominio mide la línea recta.</summary>
    private static Task<decimal?> Kilometros(ClienteOpenRouteService rutas, Local local, decimal? latitud, decimal? longitud, CancellationToken cancelar)
    {
        if (!ServicioEnvioTienda.Ofrece(local) || latitud is null || longitud is null || local.Latitud is null || local.Longitud is null)
            return Task.FromResult<decimal?>(null);
        return rutas.RecorridoKm(local.Latitud.Value, local.Longitud.Value, latitud.Value, longitud.Value, cancelar);
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
