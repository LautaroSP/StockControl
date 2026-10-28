using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using StockControl.Api.Datos;
using StockControl.Dominio;

namespace StockControl.Api.Pagos;

public static class CobrosMercadoPagoApp
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    public const string Proposito = "StockControl.MercadoPago";

    public static async Task<CobroMercadoPago> Iniciar(
        AppDbContext db,
        ClienteMercadoPagoLocal mp,
        IDataProtector protector,
        IConfiguration config,
        int idLocal,
        int nroCaja,
        int? idUsuario,
        PedidoGuardado pedido,
        decimal total)
    {
        var ids = pedido.Pagos.Select(p => p.IdMetodoPago).Distinct().ToList();
        var medios = await db.MetodosPago.Where(m => ids.Contains(m.Id) && m.Activo).ToDictionaryAsync(m => m.Id);
        if (medios.Count != ids.Count)
            throw new ErrorNegocio("El medio de pago no existe o está inactivo.");
        var conIntegracion = pedido.Pagos
            .Select(p => new PagoConIntegracion(p.IdMetodoPago, medios[p.IdMetodoPago].Integracion, p.Importe))
            .ToList();
        var modo = ServicioCobroMercadoPago.ModoDe(conIntegracion, total);
        var importe = ServicioCobroMercadoPago.ImporteDeLaOrden(conIntegracion);
        var cuenta = await db.CuentasMercadoPago.FirstOrDefaultAsync(c => c.IdLocal == idLocal);
        var puesto = await db.PuestosMercadoPago.FirstOrDefaultAsync(p => p.NroCaja == nroCaja);
        var hayAbierto = await db.CobrosMercadoPago.AnyAsync(c =>
            c.NroCaja == nroCaja && c.Modo == IntegracionPago.QrImpreso && c.Estado == EstadoCobroMercadoPago.Esperando);
        ServicioCobroMercadoPago.ValidarApertura(
            cuenta != null,
            modo,
            !string.IsNullOrWhiteSpace(puesto?.IdExterno),
            !string.IsNullOrWhiteSpace(puesto?.IdLector),
            hayAbierto);
        ServicioCobroMercadoPago.ValidarImporteQr(modo, importe);

        var cobro = new CobroMercadoPago
        {
            IdLocal = idLocal,
            NroCaja = nroCaja,
            IdUsuario = idUsuario,
            Modo = modo,
            Importe = importe,
            PedidoJson = JsonSerializer.Serialize(pedido, Json)
        };
        db.CobrosMercadoPago.Add(cobro);
        await db.SaveChangesAsync();

        try
        {
            var token = await TokenVigente(db, mp, protector, config, cuenta!);
            var referencia = CobroMercadoPago.ReferenciaDe(cobro.Id);
            var orden = modo == IntegracionPago.Point
                ? await mp.CrearOrdenPoint(token, puesto!.IdLector, referencia, importe)
                : await mp.CrearOrdenQr(token, puesto!.IdExterno, ServicioCobroMercadoPago.ModoOrden(modo), referencia, importe, [("Venta", importe, 1)]);
            cobro.IdOrdenMp = orden.Id;
            cobro.QrData = orden.QrData ?? "";
            await db.SaveChangesAsync();
            return cobro;
        }
        catch
        {
            cobro.Estado = EstadoCobroMercadoPago.Cancelado;
            await db.SaveChangesAsync();
            throw;
        }
    }

    public static async Task<CobroMercadoPago?> Abierto(
        AppDbContext db,
        ClienteMercadoPagoLocal mp,
        IDataProtector protector,
        IConfiguration config,
        int idLocal,
        int nroCaja)
    {
        var cobro = await db.CobrosMercadoPago
            .Where(c => c.NroCaja == nroCaja && c.Estado == EstadoCobroMercadoPago.Esperando && c.IdOrdenMp != "")
            .OrderByDescending(c => c.Creado)
            .FirstOrDefaultAsync();
        if (cobro == null) return null;
        try
        {
            return await Consultar(db, mp, protector, config, cobro.Id, idLocal);
        }
        catch (HttpRequestException)
        {
            return cobro;
        }
    }

    public static async Task<CobroMercadoPago?> Consultar(
        AppDbContext db,
        ClienteMercadoPagoLocal mp,
        IDataProtector protector,
        IConfiguration config,
        int id,
        int idLocal)
    {
        var cobro = await db.CobrosMercadoPago.FirstOrDefaultAsync(c => c.Id == id && c.IdLocal == idLocal);
        if (cobro == null || cobro.Estado != EstadoCobroMercadoPago.Esperando || cobro.IdOrdenMp.Length == 0)
            return cobro;
        var cuenta = await db.CuentasMercadoPago.FirstAsync(c => c.IdLocal == idLocal);
        var token = await TokenVigente(db, mp, protector, config, cuenta);
        var orden = await mp.ObtenerOrden(token, cobro.IdOrdenMp);
        await AplicarEstado(db, cobro, ServicioCobroMercadoPago.EstadoDesdeOrden(orden.Estado));
        return await db.CobrosMercadoPago.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.IdLocal == idLocal);
    }

    public static async Task<CobroMercadoPago?> AplicarAviso(
        AppDbContext db,
        ClienteMercadoPagoLocal mp,
        IDataProtector protector,
        IConfiguration config,
        string idOrden)
    {
        var cobro = await db.CobrosMercadoPago.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.IdOrdenMp == idOrden);
        if (cobro == null || cobro.Estado != EstadoCobroMercadoPago.Esperando)
            return cobro;
        var cuenta = await db.CuentasMercadoPago.IgnoreQueryFilters().FirstAsync(c => c.IdLocal == cobro.IdLocal);
        var token = await TokenVigente(db, mp, protector, config, cuenta);
        var orden = await mp.ObtenerOrden(token, idOrden);
        if (orden.Id != idOrden) return cobro;
        await AplicarEstado(db, cobro, ServicioCobroMercadoPago.EstadoDesdeOrden(orden.Estado));
        return cobro;
    }

    public static async Task<CobroMercadoPago?> Cancelar(
        AppDbContext db,
        ClienteMercadoPagoLocal mp,
        IDataProtector protector,
        IConfiguration config,
        int id,
        int idLocal)
    {
        var cobro = await db.CobrosMercadoPago.FirstOrDefaultAsync(c => c.Id == id && c.IdLocal == idLocal);
        if (cobro == null) return null;
        if (cobro.Estado == EstadoCobroMercadoPago.Esperando && cobro.IdOrdenMp.Length > 0)
        {
            var cuenta = await db.CuentasMercadoPago.FirstAsync(c => c.IdLocal == idLocal);
            var token = await TokenVigente(db, mp, protector, config, cuenta);
            await mp.CancelarOrden(token, cobro.IdOrdenMp);
        }
        ServicioCobroMercadoPago.Cancelar(cobro);
        await db.SaveChangesAsync();
        return cobro;
    }

    public static async Task PrepararPuesto(
        AppDbContext db,
        ClienteMercadoPagoLocal mp,
        IDataProtector protector,
        IConfiguration config,
        int idLocal,
        int nroCaja,
        string nombreLocal)
    {
        var cuenta = await db.CuentasMercadoPago.FirstOrDefaultAsync(c => c.IdLocal == idLocal)
            ?? throw new ErrorNegocio("Este local no tiene Mercado Pago conectado.");
        var token = await TokenVigente(db, mp, protector, config, cuenta);
        var idSucursalExterno = PuestoMercadoPago.IdSucursalDe(idLocal);
        var idSucursal = await mp.AsegurarSucursal(token, cuenta.IdUsuarioMp, idSucursalExterno, nombreLocal);
        var idExterno = PuestoMercadoPago.IdExternoDe(idLocal, nroCaja);
        var caja = await mp.AsegurarCaja(token, idSucursal, idExterno, $"Puesto {nroCaja}");
        var puesto = await db.PuestosMercadoPago.FirstOrDefaultAsync(p => p.NroCaja == nroCaja);
        if (puesto == null)
        {
            puesto = new PuestoMercadoPago { IdLocal = idLocal, NroCaja = nroCaja };
            db.PuestosMercadoPago.Add(puesto);
        }
        puesto.IdSucursal = idSucursal;
        puesto.IdExterno = caja.IdExterno;
        puesto.UrlQr = caja.UrlQr;
        await db.SaveChangesAsync();
    }

    public static async Task GuardarLector(AppDbContext db, int idLocal, int nroCaja, string idLector)
    {
        var puesto = await db.PuestosMercadoPago.FirstOrDefaultAsync(p => p.NroCaja == nroCaja);
        if (puesto == null)
        {
            puesto = new PuestoMercadoPago { IdLocal = idLocal, NroCaja = nroCaja };
            db.PuestosMercadoPago.Add(puesto);
        }
        puesto.IdLector = idLector.Trim();
        await db.SaveChangesAsync();
    }

    public static async Task<string> TokenVigente(
        AppDbContext db,
        ClienteMercadoPagoLocal mp,
        IDataProtector protector,
        IConfiguration config,
        CuentaMercadoPago cuenta)
    {
        if (cuenta.Vence > DateTimeOffset.UtcNow.AddMinutes(2))
            return protector.Unprotect(cuenta.TokenAcceso);
        var renovado = await mp.Renovar(
            Exigir(config, "MercadoPago:ClientId"),
            Exigir(config, "MercadoPago:ClientSecret"),
            protector.Unprotect(cuenta.TokenRenovacion));
        GuardarToken(protector, cuenta, renovado);
        await db.SaveChangesAsync();
        return renovado.Acceso;
    }

    public static void GuardarToken(IDataProtector protector, CuentaMercadoPago cuenta, TokenMercadoPago token)
    {
        if (token.Acceso.Length == 0 || token.Renovacion.Length == 0)
            throw new InvalidOperationException("Mercado Pago no entregó los tokens.");
        cuenta.TokenAcceso = protector.Protect(token.Acceso);
        cuenta.TokenRenovacion = protector.Protect(token.Renovacion);
        cuenta.Vence = token.Vence;
        if (token.IdUsuario.Length > 0)
            cuenta.IdUsuarioMp = token.IdUsuario;
    }

    public static string Exigir(IConfiguration config, string clave)
    {
        var valor = config[clave];
        if (string.IsNullOrWhiteSpace(valor))
            throw new InvalidOperationException($"Falta {clave}.");
        return valor;
    }

    private static async Task AplicarEstado(AppDbContext db, CobroMercadoPago cobro, string estado)
    {
        if (estado == EstadoCobroMercadoPago.Esperando)
            return;
        if (estado == EstadoCobroMercadoPago.Pagado)
        {
            var id = cobro.Id;
            db.Entry(cobro).State = EntityState.Detached;
            await Confirmar(db, id);
            return;
        }
        if (estado == EstadoCobroMercadoPago.Cancelado)
            ServicioCobroMercadoPago.Cancelar(cobro);
        else if (estado == EstadoCobroMercadoPago.Vencido)
            ServicioCobroMercadoPago.Vencer(cobro);
        await db.SaveChangesAsync();
    }

    private static async Task Confirmar(AppDbContext db, int idCobro)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        await db.Database.SqlQuery<long>($"SELECT pg_advisory_xact_lock({idCobro}) AS \"Value\"").ToListAsync();
        var cobro = await db.CobrosMercadoPago.IgnoreQueryFilters().FirstAsync(c => c.Id == idCobro);
        if (!ServicioCobroMercadoPago.IntentarMarcarPagado(cobro))
        {
            await tx.CommitAsync();
            return;
        }
        var pedido = JsonSerializer.Deserialize<PedidoGuardado>(cobro.PedidoJson, Json)
            ?? throw new ErrorNegocio("El cobro no tiene el carrito.");
        var productos = await db.Productos.IgnoreQueryFilters().Where(p => p.IdLocal == cobro.IdLocal).ToListAsync();
        var rigido = (await db.Configuracion.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.IdLocal == cobro.IdLocal && c.Clave == "StockRigido"))?.Valor == "1";
        var hoy = DiaArgentina.Hoy();
        var resultado = ServicioCobro.Cobrar(new PedidoCobro
        {
            Ofertas = await OfertasVigentes.DelDia(db, cobro.IdLocal, hoy),
            Combos = await OfertasVigentes.CombosDelDia(db, cobro.IdLocal, hoy),
            Hoy = hoy,
            SinOfertas = pedido.SinOfertas,
            ProductosDelLocal = productos,
            Items = pedido.Items,
            DescuentoPorcentaje = pedido.DescuentoPorcentaje,
            CobrarAlCosto = pedido.CobrarAlCosto,
            StockRigido = rigido,
            Reservado = await ReservasTienda.DelLocal(db, cobro.IdLocal),
            MetodoPago = "Mercado Pago"
        });
        var venta = await RegistrarVenta.Guardar(db, cobro.IdLocal, cobro.IdUsuario, cobro.NroCaja, resultado, pedido.Pagos);
        if (pedido.IdPedidoEspera is int idEspera)
            await db.PedidosEspera.IgnoreQueryFilters()
                .Where(p => p.Id == idEspera && p.IdLocal == cobro.IdLocal && p.NroCaja == cobro.NroCaja)
                .ExecuteDeleteAsync();
        await db.SaveChangesAsync();
        cobro.IdInformeVenta = venta.IdInformeVenta;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

}
