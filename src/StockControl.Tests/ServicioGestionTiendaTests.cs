using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioGestionTiendaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 11, 1, 15, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Vence = Ahora.AddDays(20);

    private static PedidoTienda Pedido(string medio, string estadoPago, string estado, decimal gestion = 1000) => new()
    {
        Id = 7,
        IdLocal = 1,
        MedioPago = medio,
        EstadoPago = estadoPago,
        Estado = estado,
        Gestion = gestion,
        Subtotal = 3000,
        Total = 3000
    };

    private static Local AlDia() => new()
    {
        IdLocal = 1,
        Nombre = "Almagro",
        AliasTienda = "almagro",
        EstadoTienda = ServicioTienda.Activa,
        EstadoAbono = ServicioAbono.AlDia,
        Vence = Ahora.AddMonths(1)
    };

    private static GestionTienda Deuda(DateTimeOffset vencePeriodo, decimal importe = 1000) => new()
    {
        IdLocal = 1,
        IdPedido = 7,
        Importe = importe,
        Medio = ServicioPagoPedidoTienda.Transferencia,
        Estado = GestionTienda.Pendiente,
        Nacida = Ahora,
        VencePeriodo = vencePeriodo
    };

    [Fact]
    public void Transferencia_confirmada_genera_deuda_con_el_importe_congelado()
    {
        var pedido = Pedido(ServicioPagoPedidoTienda.Transferencia, ServicioPagoPedidoTienda.Pagado, ServicioPedidosTienda.Nuevo, gestion: 600);

        var deuda = ServicioGestionTienda.Nacer(pedido, Vence, Ahora, yaExiste: false);

        Assert.NotNull(deuda);
        Assert.Equal(600, deuda.Importe);
        Assert.Equal(GestionTienda.Pendiente, deuda.Estado);
        Assert.Equal(Vence, deuda.VencePeriodo);
        Assert.Equal(ServicioPagoPedidoTienda.Transferencia, deuda.Medio);
    }

    [Fact]
    public void Coordinar_entregado_genera_deuda()
    {
        var pedido = Pedido(ServicioPagoPedidoTienda.Coordinar, ServicioPagoPedidoTienda.SinPago, ServicioPedidosTienda.Entregado);

        var deuda = ServicioGestionTienda.Nacer(pedido, Vence, Ahora, yaExiste: false);

        Assert.NotNull(deuda);
        Assert.Equal(1000, deuda.Importe);
        Assert.Equal(ServicioPagoPedidoTienda.Coordinar, deuda.Medio);
    }

    [Fact]
    public void Coordinar_sin_entregar_no_genera_deuda()
    {
        var pedido = Pedido(ServicioPagoPedidoTienda.Coordinar, ServicioPagoPedidoTienda.SinPago, ServicioPedidosTienda.Nuevo);

        Assert.Null(ServicioGestionTienda.Nacer(pedido, Vence, Ahora, yaExiste: false));
    }

    [Fact]
    public void Mercado_Pago_no_genera_deuda()
    {
        var pedido = Pedido(ServicioPagoPedidoTienda.MercadoPago, ServicioPagoPedidoTienda.Pagado, ServicioPedidosTienda.Nuevo);

        Assert.Null(ServicioGestionTienda.Nacer(pedido, Vence, Ahora, yaExiste: false));
    }

    [Fact]
    public void Cancelado_o_vencido_no_genera_deuda()
    {
        var cancelado = Pedido(ServicioPagoPedidoTienda.Transferencia, ServicioPagoPedidoTienda.Pendiente, ServicioPedidosTienda.Cancelado);
        var vencido = Pedido(ServicioPagoPedidoTienda.Transferencia, ServicioPagoPedidoTienda.Pendiente, ServicioPedidosTienda.Cancelado);

        Assert.Null(ServicioGestionTienda.Nacer(cancelado, Vence, Ahora, yaExiste: false));
        Assert.Null(ServicioGestionTienda.Nacer(vencido, Vence, Ahora, yaExiste: false));
    }

    [Fact]
    public void Local_eximido_no_genera_deuda()
    {
        var pedido = Pedido(ServicioPagoPedidoTienda.Transferencia, ServicioPagoPedidoTienda.Pagado, ServicioPedidosTienda.Nuevo, gestion: 0);

        Assert.Null(ServicioGestionTienda.Nacer(pedido, Vence, Ahora, yaExiste: false));
    }

    [Fact]
    public void Nacer_es_idempotente()
    {
        var pedido = Pedido(ServicioPagoPedidoTienda.Transferencia, ServicioPagoPedidoTienda.Pagado, ServicioPedidosTienda.Nuevo);

        Assert.Null(ServicioGestionTienda.Nacer(pedido, Vence, Ahora, yaExiste: true));
    }

    [Fact]
    public void Devolver_una_transferencia_anula_la_deuda_pendiente()
    {
        var deuda = Deuda(Vence);

        ServicioGestionTienda.Anular(deuda);

        Assert.Equal(GestionTienda.Anulada, deuda.Estado);
    }

    [Fact]
    public void Devolver_no_anula_una_deuda_ya_saldada()
    {
        var deuda = Deuda(Vence);
        deuda.Estado = GestionTienda.Saldada;

        ServicioGestionTienda.Anular(deuda);

        Assert.Equal(GestionTienda.Saldada, deuda.Estado);
    }

    [Fact]
    public void Devolver_con_pago_en_curso_no_anula()
    {
        var deuda = Deuda(Vence);
        deuda.IdAbono = 4;

        Assert.Throws<ErrorNegocio>(() => ServicioGestionTienda.Anular(deuda));
        Assert.Equal(GestionTienda.Pendiente, deuda.Estado);
    }

    [Fact]
    public void El_pago_del_mes_y_el_debito_no_saldan_la_deuda()
    {
        var local = AlDia();
        var deuda = Deuda(Vence);
        var mes = ServicioAbono.NuevoAbono(local.IdLocal, Abono.OrigenMp, Ahora);
        var debito = ServicioAbono.NuevoAbono(local.IdLocal, Abono.OrigenDebito, Ahora);

        ServicioAbono.Acreditar(local, mes, Ahora, "mes");
        local.Vence = Vence;
        local.EstadoAbono = ServicioAbono.AlDia;
        ServicioAbono.Acreditar(local, debito, Ahora, "debito");

        Assert.Equal(ServicioAbono.PrecioMensual, mes.Importe);
        Assert.Equal(ServicioAbono.PrecioMensual, debito.Importe);
        Assert.Equal(GestionTienda.Pendiente, deuda.Estado);
        Assert.Null(deuda.IdAbono);
    }

    [Fact]
    public void El_cobro_aparte_salda_sin_cambiar_el_vencimiento()
    {
        var local = AlDia();
        var vence = local.Vence;
        var deuda = Deuda(Vence, importe: 600);
        var otra = Deuda(Vence, importe: 1000);
        otra.IdPedido = 8;
        var abono = ServicioGestionTienda.NuevoCobro(local.IdLocal, [deuda, otra], Ahora);
        abono.Id = 9;
        ServicioGestionTienda.Reservar([deuda, otra], abono.Id);

        ServicioGestionTienda.AcreditarCobro(abono, [deuda, otra], Ahora, "gestion-1");

        Assert.Equal(1600, abono.Importe);
        Assert.Equal(Abono.Aprobado, abono.Estado);
        Assert.Equal(GestionTienda.Saldada, deuda.Estado);
        Assert.Equal(GestionTienda.Saldada, otra.Estado);
        Assert.Equal(vence, local.Vence);
        Assert.Equal(ServicioAbono.AlDia, local.EstadoAbono);
    }

    [Fact]
    public void El_admin_salda_y_al_anular_la_deuda_vuelve()
    {
        var local = AlDia();
        var vence = local.Vence;
        var deuda = Deuda(Vence);
        var saldo = ServicioGestionTienda.PrepararSaldoAdmin(local.IdLocal, [deuda], Ahora);
        saldo.Id = 3;

        ServicioGestionTienda.MarcarSaldadas([deuda], saldo.Id, Ahora);
        Assert.Equal(GestionTienda.Saldada, deuda.Estado);
        Assert.Equal(vence, local.Vence);

        ServicioGestionTienda.AnularSaldoAdmin(saldo, [deuda]);

        Assert.Equal(GestionTienda.Pendiente, deuda.Estado);
        Assert.Null(deuda.IdAbono);
        Assert.Equal(Abono.Anulado, saldo.Estado);
        Assert.Equal(vence, local.Vence);
    }

    [Fact]
    public void Dentro_del_mes_y_a_los_6_dias_la_tienda_sigue()
    {
        var deuda = Deuda(Ahora);
        var local = AlDia();

        Assert.False(ServicioGestionTienda.CierraTienda([deuda], Ahora.AddDays(-1)));
        Assert.False(ServicioGestionTienda.CierraTienda([deuda], Ahora.AddDays(6)));
        Assert.Equal(ServicioTienda.VistaCatalogo, ServicioTienda.Vista(local, Ahora.AddDays(6)));
    }

    [Fact]
    public void Al_dia_8_se_cierra_la_tienda_y_la_caja_puede_cobrar()
    {
        var deuda = Deuda(Ahora);
        var local = AlDia();
        var tarde = Ahora.AddDays(8);

        Assert.True(ServicioGestionTienda.CierraTienda([deuda], tarde));
        Assert.True(ServicioAbono.Estado(local, tarde).PuedeOperarCaja);
        Assert.Equal(ServicioTienda.VistaCerrada, ServicioTienda.Vista(local, tarde, gestionVencida: true));
        Assert.Equal("gestion", ServicioTienda.MotivoCierre(local, tarde, gestionVencida: true));
    }

    [Fact]
    public void Saldar_la_deuda_reabre_la_tienda()
    {
        var deuda = Deuda(Ahora);
        var local = AlDia();
        var tarde = Ahora.AddDays(8);
        var saldo = ServicioGestionTienda.PrepararSaldoAdmin(local.IdLocal, [deuda], tarde);
        saldo.Id = 5;
        ServicioGestionTienda.MarcarSaldadas([deuda], saldo.Id, tarde);

        Assert.False(ServicioGestionTienda.CierraTienda([deuda], tarde));
        Assert.Equal(ServicioTienda.VistaCatalogo, ServicioTienda.Vista(local, tarde));
    }

    [Fact]
    public void El_abono_vencido_sigue_cerrando_la_tienda_y_la_caja()
    {
        var local = AlDia();
        local.Vence = Ahora.AddDays(-1);

        Assert.False(ServicioAbono.Estado(local, Ahora).PuedeOperarCaja);
        Assert.Equal(ServicioTienda.VistaCerrada, ServicioTienda.Vista(local, Ahora, gestionVencida: true));
        Assert.Equal("abono", ServicioTienda.MotivoCierre(local, Ahora, gestionVencida: true));
    }
}
