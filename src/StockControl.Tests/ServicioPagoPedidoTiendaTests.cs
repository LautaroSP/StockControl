using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioPagoPedidoTiendaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 30, 12, 0, 0, TimeSpan.FromHours(-3));
    private const string CuentaLocal = "555";

    private static Local Tienda(bool aCargoComprador = false, bool exento = false) => new()
    {
        IdLocal = 1,
        Nombre = "Almagro",
        EstadoAbono = ServicioAbono.AlDia,
        Vence = Ahora.AddDays(20),
        AliasTienda = "almagro",
        EstadoTienda = ServicioTienda.Activa,
        GestionACargoComprador = aCargoComprador,
        ExentoGestion = exento
    };

    private static Producto Producto(decimal precio = 1500, decimal stock = 10) => new()
    {
        Id = 1,
        IdLocal = 1,
        Codigo = "7791",
        Nombre = "Producto 1",
        Cantidad = stock,
        Precio = precio,
        Costo = 1000,
        Publicado = true
    };

    private static PedidoTienda Crear(Local? local = null, decimal precio = 1500, int cantidad = 2,
        string medio = ServicioPagoPedidoTienda.MercadoPago, Producto? producto = null)
    {
        var pedido = ServicioPedidosTienda.Crear(
            local ?? Tienda(), Ahora, 1, new("Ana Pérez", "11 4444-5555", "ana@correo.com"),
            [new(1, cantidad)], [producto ?? Producto(precio)], new Dictionary<int, decimal>(), false, medio).Pedido;
        pedido.Id = 7;
        return pedido;
    }

    private static PagoMercadoPagoTienda Pago(PedidoTienda pedido, string estado = "approved", decimal? importe = null,
        string id = "900", string moneda = "ARS", string? referencia = null, string cobrador = CuentaLocal) =>
        new(id, estado, moneda, importe ?? pedido.Total, referencia ?? ServicioPagoPedidoTienda.Referencia(pedido.Id), cobrador);

    private static PedidoTienda Pagado(Local? local = null)
    {
        var pedido = Crear(local);
        ServicioPagoPedidoTienda.Aplicar(pedido, Pago(pedido), CuentaLocal, Ahora);
        return pedido;
    }

    [Fact]
    public void Gestion_incluida_no_cambia_el_total_del_comprador()
    {
        var pedido = Crear(Tienda(aCargoComprador: false));

        Assert.Equal(1000, pedido.Gestion);
        Assert.Equal(3000, pedido.Subtotal);
        Assert.Equal(3000, pedido.Total);
    }

    [Fact]
    public void Gestion_a_cargo_del_comprador_suma_al_total()
    {
        var pedido = Crear(Tienda(aCargoComprador: true));

        Assert.Equal(1000, pedido.Gestion);
        Assert.True(pedido.GestionACargoComprador);
        Assert.Equal(4000, pedido.Total);
    }

    [Fact]
    public void La_gestion_es_la_misma_sin_importar_el_tamano_del_carrito()
    {
        Assert.Equal(1000, Crear(precio: 50_000, cantidad: 10).Gestion);
    }

    [Fact]
    public void Si_el_subtotal_es_menor_que_el_importe_la_gestion_es_el_subtotal()
    {
        var pedido = Crear(Tienda(aCargoComprador: true), precio: 300, cantidad: 2);

        Assert.Equal(600, pedido.Gestion);
        Assert.Equal(1200, pedido.Total);
    }

    [Fact]
    public void Local_eximido_por_el_admin_no_cobra_gestion()
    {
        var pedido = Crear(Tienda(aCargoComprador: true, exento: true));

        Assert.Equal(0, pedido.Gestion);
        Assert.Equal(3000, pedido.Total);
    }

    [Fact]
    public void La_gestion_queda_congelada_en_el_pedido()
    {
        var local = Tienda(aCargoComprador: true);
        var pedido = Crear(local);

        local.ExentoGestion = true;
        local.GestionACargoComprador = false;

        Assert.Equal(1000, pedido.Gestion);
        Assert.Equal(4000, pedido.Total);
    }

    [Fact]
    public void Coordinar_con_el_local_no_espera_pago_ni_cobra_gestion_por_MP()
    {
        var pedido = Crear(Tienda(aCargoComprador: true), medio: ServicioPagoPedidoTienda.Coordinar);

        Assert.Equal(ServicioPagoPedidoTienda.SinPago, pedido.EstadoPago);
        Assert.Equal(0, pedido.Gestion);
        Assert.Equal(3000, pedido.Total);
    }

    [Fact]
    public void Con_Mercado_Pago_el_pedido_espera_el_pago()
    {
        var pedido = Crear();

        Assert.Equal(ServicioPagoPedidoTienda.Pendiente, pedido.EstadoPago);
        Assert.True(ServicioPagoPedidoTienda.EsperaPago(pedido));
    }

    [Fact]
    public void Sin_cuenta_de_Mercado_Pago_no_se_ofrece_el_medio()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioPagoPedidoTienda.NormalizarMedio(ServicioPagoPedidoTienda.MercadoPago, aceptaMercadoPago: false, aceptaTransferencia: true));
        Assert.Equal(ServicioPagoPedidoTienda.Coordinar, ServicioPagoPedidoTienda.NormalizarMedio(null, aceptaMercadoPago: false, aceptaTransferencia: false));
    }

    [Fact]
    public void Un_pago_aprobado_y_valido_marca_el_pedido_pagado()
    {
        var pedido = Crear();

        var resultado = ServicioPagoPedidoTienda.Aplicar(pedido, Pago(pedido), CuentaLocal, Ahora);

        Assert.Equal(ResultadoPagoTienda.Acreditado, resultado);
        Assert.Equal(ServicioPagoPedidoTienda.Pagado, pedido.EstadoPago);
        Assert.Equal("900", pedido.IdPagoMp);
        Assert.Equal(Ahora, pedido.Pagado);
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("in_process")]
    [InlineData("rejected")]
    public void Un_pago_sin_aprobar_no_marca_pagado(string estado)
    {
        var pedido = Crear();

        var resultado = ServicioPagoPedidoTienda.Aplicar(pedido, Pago(pedido, estado), CuentaLocal, Ahora);

        Assert.Equal(ResultadoPagoTienda.SinAprobar, resultado);
        Assert.Equal(ServicioPagoPedidoTienda.Pendiente, pedido.EstadoPago);
    }

    [Fact]
    public void Un_pago_por_otro_importe_no_marca_pagado()
    {
        var pedido = Crear();

        Assert.Throws<ErrorNegocio>(() => ServicioPagoPedidoTienda.Aplicar(pedido, Pago(pedido, importe: 1), CuentaLocal, Ahora));
        Assert.Equal(ServicioPagoPedidoTienda.Pendiente, pedido.EstadoPago);
    }

    [Fact]
    public void Un_pago_en_otra_moneda_no_marca_pagado()
    {
        var pedido = Crear();

        Assert.Throws<ErrorNegocio>(() => ServicioPagoPedidoTienda.Aplicar(pedido, Pago(pedido, moneda: "USD"), CuentaLocal, Ahora));
    }

    [Fact]
    public void Un_pago_de_otro_pedido_no_marca_pagado()
    {
        var pedido = Crear();

        Assert.Throws<ErrorNegocio>(() => ServicioPagoPedidoTienda.Aplicar(pedido, Pago(pedido, referencia: "pedido-tienda-8"), CuentaLocal, Ahora));
        Assert.Throws<ErrorNegocio>(() => ServicioPagoPedidoTienda.Aplicar(pedido, Pago(pedido, referencia: "abono-7"), CuentaLocal, Ahora));
    }

    [Fact]
    public void Un_pago_que_entro_en_otra_cuenta_no_marca_pagado()
    {
        var pedido = Crear();

        Assert.Throws<ErrorNegocio>(() => ServicioPagoPedidoTienda.Aplicar(pedido, Pago(pedido, cobrador: "999"), CuentaLocal, Ahora));
    }

    [Fact]
    public void El_mismo_aviso_dos_veces_no_duplica()
    {
        var pedido = Pagado();

        var resultado = ServicioPagoPedidoTienda.Aplicar(pedido, Pago(pedido), CuentaLocal, Ahora.AddMinutes(1));

        Assert.Equal(ResultadoPagoTienda.Repetido, resultado);
        Assert.Equal(Ahora, pedido.Pagado);
    }

    [Fact]
    public void Un_segundo_pago_del_mismo_pedido_se_devuelve()
    {
        var pedido = Pagado();

        var resultado = ServicioPagoPedidoTienda.Aplicar(pedido, Pago(pedido, id: "901"), CuentaLocal, Ahora);

        Assert.Equal(ResultadoPagoTienda.Devolver, resultado);
        Assert.Equal("900", pedido.IdPagoMp);
    }

    [Fact]
    public void Un_pago_que_llega_a_un_pedido_cancelado_se_devuelve()
    {
        var pedido = Crear();
        ServicioPedidosTienda.Cancelar(pedido, Ahora);

        var resultado = ServicioPagoPedidoTienda.Aplicar(pedido, Pago(pedido), CuentaLocal, Ahora);
        ServicioPagoPedidoTienda.RegistrarDevolucion(pedido, "900");

        Assert.Equal(ResultadoPagoTienda.Devolver, resultado);
        Assert.Equal(ServicioPagoPedidoTienda.Reembolsado, pedido.EstadoPago);
        Assert.Equal(0, ServicioGestionTienda.Cobrada([pedido]));
    }

    [Fact]
    public void Un_pago_pendiente_vence_a_los_30_minutos_y_libera_el_stock()
    {
        var pedido = Crear();
        var despues = Ahora + ServicioPagoPedidoTienda.Plazo;

        Assert.False(ServicioPagoPedidoTienda.Vencido(pedido, despues.AddSeconds(-1)));
        ServicioPagoPedidoTienda.Vencer(pedido, despues);

        Assert.Equal(ServicioPedidosTienda.Cancelado, pedido.Estado);
        Assert.Empty(ServicioPedidosTienda.Reservado([pedido]));
    }

    [Fact]
    public void Un_pedido_pagado_o_sin_MP_no_vence()
    {
        var tarde = Ahora.AddHours(5);

        Assert.False(ServicioPagoPedidoTienda.Vencido(Pagado(), tarde));
        Assert.False(ServicioPagoPedidoTienda.Vencido(Crear(medio: ServicioPagoPedidoTienda.Coordinar), tarde));
    }

    [Fact]
    public void El_pago_pendiente_reserva_stock()
    {
        Assert.Equal(2, ServicioPedidosTienda.Reservado([Crear()])[1]);
    }

    [Fact]
    public void Un_pedido_que_espera_el_pago_no_se_prepara_ni_se_entrega()
    {
        var pedido = Crear();

        Assert.Throws<ErrorNegocio>(() => ServicioPedidosTienda.Avanzar(pedido, ServicioPedidosTienda.Preparando, Ahora));
        Assert.Throws<ErrorNegocio>(() => ServicioEntregaPedidoTienda.Entregar(pedido, [Producto()], Ahora));
        Assert.Equal(ServicioPedidosTienda.Nuevo, pedido.Estado);
    }

    [Fact]
    public void El_comprador_no_cancela_un_pedido_pagado()
    {
        var pedido = Pagado();

        Assert.Throws<ErrorNegocio>(() => ServicioPedidosTienda.Cancelar(pedido, Ahora));
        Assert.False(ServicioPedidosTienda.PuedeCancelar(pedido));
    }

    [Fact]
    public void El_comprador_cancela_un_pedido_que_todavia_no_pago()
    {
        var pedido = Crear();

        ServicioPedidosTienda.Cancelar(pedido, Ahora);

        Assert.Equal(ServicioPedidosTienda.Cancelado, pedido.Estado);
    }

    [Fact]
    public void El_local_no_cancela_un_pedido_pagado_sin_reembolso()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioPedidosTienda.CancelarDelLocal(Pagado(), Ahora));
    }

    [Fact]
    public void Cancelar_con_reembolso_libera_el_stock_y_no_cobra_gestion()
    {
        var pedido = Pagado();
        Assert.Equal(1000, ServicioGestionTienda.Cobrada([pedido]));

        ServicioPagoPedidoTienda.CancelarConReembolso(pedido, Ahora);

        Assert.Equal(ServicioPedidosTienda.Cancelado, pedido.Estado);
        Assert.Equal(ServicioPagoPedidoTienda.Reembolsado, pedido.EstadoPago);
        Assert.Empty(ServicioPedidosTienda.Reservado([pedido]));
        Assert.Equal(0, ServicioGestionTienda.Cobrada([pedido]));
    }

    [Fact]
    public void Solo_se_reembolsa_un_pedido_pagado()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioPagoPedidoTienda.CancelarConReembolso(Crear(), Ahora));
    }

    [Fact]
    public void Un_pedido_entregado_no_se_reembolsa()
    {
        var pedido = Pagado();
        ServicioEntregaPedidoTienda.Entregar(pedido, [Producto()], Ahora);

        Assert.Throws<ErrorNegocio>(() => ServicioPagoPedidoTienda.CancelarConReembolso(pedido, Ahora));
    }

    [Fact]
    public void Entregar_un_pedido_pagado_vende_el_subtotal_sin_la_gestion()
    {
        var pedido = Pagado(Tienda(aCargoComprador: true));

        var venta = ServicioEntregaPedidoTienda.Entregar(pedido, [Producto()], Ahora);

        Assert.Equal(4000, pedido.Total);
        Assert.Equal(3000, venta.Total);
        Assert.Equal(3000, venta.Subtotal);
    }

    [Fact]
    public void La_gestion_cobrada_suma_solo_los_pedidos_pagados()
    {
        var pagado = Pagado();
        var pendiente = Crear();
        var coordinado = Crear(medio: ServicioPagoPedidoTienda.Coordinar);

        Assert.Equal(1000, ServicioGestionTienda.Cobrada([pagado, pendiente, coordinado]));
    }
}
