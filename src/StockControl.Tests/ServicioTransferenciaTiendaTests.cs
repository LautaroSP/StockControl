using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioTransferenciaTiendaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 30, 12, 0, 0, TimeSpan.FromHours(-3));
    private const string CbuValido = "2850590940090418135201";

    private static Local Tienda(int? plazoHoras = 24, bool aCargoComprador = false, bool exento = false) => new()
    {
        IdLocal = 1,
        Nombre = "Almagro",
        EstadoAbono = ServicioAbono.AlDia,
        Vence = Ahora.AddDays(20),
        AliasTienda = "almagro",
        EstadoTienda = ServicioTienda.Activa,
        GestionACargoComprador = aCargoComprador,
        ExentoGestion = exento,
        CbuTienda = CbuValido,
        TitularTienda = "Ana Pérez",
        PlazoTransferenciaHoras = plazoHoras
    };

    private static PedidoTienda Crear(Local? local = null)
    {
        var pedido = ServicioPedidosTienda.Crear(
            local ?? Tienda(), Ahora, 1, new("Juan Gómez", "11 4444-5555", "juan@correo.com"),
            [new(1, 2)], [Producto()], new Dictionary<int, decimal>(), false, ServicioPagoPedidoTienda.Transferencia).Pedido;
        pedido.Id = 7;
        return pedido;
    }

    private static Producto Producto() => new()
    {
        Id = 1,
        IdLocal = 1,
        Codigo = "7791",
        Nombre = "Producto 1",
        Cantidad = 10,
        Precio = 1500,
        Costo = 1000,
        Publicado = true
    };

    private static PedidoTienda Confirmado(Local? local = null)
    {
        var pedido = Crear(local);
        ServicioTransferenciaTienda.Confirmar(pedido, Ahora.AddHours(1));
        return pedido;
    }

    [Fact]
    public void Sin_CBU_ni_alias_no_se_ofrece_transferencia()
    {
        var local = Tienda();
        local.CbuTienda = "";

        Assert.False(ServicioTransferenciaTienda.Acepta(local));
        Assert.Throws<ErrorNegocio>(() => ServicioPagoPedidoTienda.NormalizarMedio(ServicioPagoPedidoTienda.Transferencia, aceptaMercadoPago: true, aceptaTransferencia: false));
        Assert.Throws<ErrorNegocio>(() => Crear(local));
    }

    [Fact]
    public void Sin_titular_no_se_ofrece_transferencia()
    {
        var local = Tienda();
        local.TitularTienda = "";

        Assert.False(ServicioTransferenciaTienda.Acepta(local));
    }

    [Fact]
    public void Con_alias_y_titular_alcanza_para_ofrecer_transferencia()
    {
        var local = Tienda();
        ServicioTransferenciaTienda.AsignarDatos(local, "", "casa.perro.mesa", "Ana Pérez", 24);

        Assert.True(ServicioTransferenciaTienda.Acepta(local));
        Assert.Equal("CASA.PERRO.MESA", local.AliasCbuTienda);
    }

    [Fact]
    public void Configuracion_guarda_CBU_alias_titular_y_plazo()
    {
        var local = new Local();

        ServicioTransferenciaTienda.AsignarDatos(local, "2850590-940090418135201 ", "casa.perro.mesa", " Ana Pérez ", null);

        Assert.Equal(CbuValido, local.CbuTienda);
        Assert.Equal("Ana Pérez", local.TitularTienda);
        Assert.Null(local.PlazoTransferenciaHoras);
    }

    [Theory]
    [InlineData("2850590940090418135202")]
    [InlineData("2850591940090418135201")]
    [InlineData("285059094009041813520")]
    [InlineData("28505909400904181352AB")]
    public void Un_CBU_con_digitos_verificadores_malos_se_rechaza(string cbu)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioTransferenciaTienda.NormalizarCbu(cbu));
    }

    [Theory]
    [InlineData("corto")]
    [InlineData("alias.con.mas.de.veinte")]
    [InlineData("año.perro.mesa")]
    public void Un_alias_invalido_se_rechaza(string alias)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioTransferenciaTienda.NormalizarAlias(alias));
    }

    [Fact]
    public void CBU_sin_titular_no_se_guarda()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioTransferenciaTienda.AsignarDatos(new Local(), CbuValido, "", "", 24));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(169)]
    public void El_plazo_va_de_1_a_168_horas(int horas)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioTransferenciaTienda.AsignarDatos(Tienda(), CbuValido, "", "Ana Pérez", horas));
    }

    [Fact]
    public void El_pedido_queda_a_confirmar_y_reserva_stock()
    {
        var pedido = Crear();

        Assert.True(ServicioTransferenciaTienda.EsperaConfirmacion(pedido));
        Assert.True(ServicioPagoPedidoTienda.EsperaPago(pedido));
        Assert.False(ServicioPagoPedidoTienda.EsperaMercadoPago(pedido));
        Assert.Equal(2, ServicioPedidosTienda.Reservado([pedido])[1]);
    }

    [Fact]
    public void La_transferencia_congela_la_gestion_a_cargo_del_comprador()
    {
        var pedido = Crear(Tienda(aCargoComprador: true));

        Assert.Equal(1000, pedido.Gestion);
        Assert.Equal(4000, pedido.Total);
    }

    [Fact]
    public void Vence_segun_el_plazo_del_dueno_y_libera_el_stock()
    {
        var pedido = Crear(Tienda(plazoHoras: 6));
        var vence = Ahora.AddHours(6);

        Assert.Equal(vence, pedido.VencePago);
        Assert.False(ServicioPagoPedidoTienda.Vencido(pedido, vence.AddSeconds(-1)));
        ServicioPagoPedidoTienda.Vencer(pedido, vence);

        Assert.Equal(ServicioPedidosTienda.Cancelado, pedido.Estado);
        Assert.Empty(ServicioPedidosTienda.Reservado([pedido]));
    }

    [Fact]
    public void Sin_vencimiento_no_vence_nunca()
    {
        var pedido = Crear(Tienda(plazoHoras: null));

        Assert.Null(pedido.VencePago);
        Assert.False(ServicioPagoPedidoTienda.Vencido(pedido, Ahora.AddYears(1)));
    }

    [Fact]
    public void Cambiar_el_plazo_despues_no_altera_el_pedido()
    {
        var local = Tienda(plazoHoras: 6);
        var pedido = Crear(local);

        local.PlazoTransferenciaHoras = null;

        Assert.True(ServicioPagoPedidoTienda.Vencido(pedido, Ahora.AddHours(6)));
    }

    [Fact]
    public void Confirmar_la_transferencia_pasa_el_pedido_a_pagado()
    {
        var pedido = Confirmado();

        Assert.Equal(ServicioPagoPedidoTienda.Pagado, pedido.EstadoPago);
        Assert.Equal(Ahora.AddHours(1), pedido.Pagado);
        Assert.False(ServicioPagoPedidoTienda.Vencido(pedido, Ahora.AddDays(5)));
        Assert.Equal(2, ServicioPedidosTienda.Reservado([pedido])[1]);
    }

    [Fact]
    public void No_se_confirma_una_transferencia_vencida()
    {
        var pedido = Crear(Tienda(plazoHoras: 2));

        Assert.Throws<ErrorNegocio>(() => ServicioTransferenciaTienda.Confirmar(pedido, Ahora.AddHours(2)));
        Assert.Equal(ServicioPagoPedidoTienda.Pendiente, pedido.EstadoPago);
    }

    [Fact]
    public void No_se_confirma_un_pedido_cancelado()
    {
        var pedido = Crear();
        ServicioPedidosTienda.Cancelar(pedido, Ahora);

        Assert.Throws<ErrorNegocio>(() => ServicioTransferenciaTienda.Confirmar(pedido, Ahora));
    }

    [Fact]
    public void No_se_confirma_un_pedido_de_otro_medio()
    {
        var pedido = ServicioPedidosTienda.Crear(
            Tienda(), Ahora, 1, new("Juan Gómez", "11 4444-5555", "juan@correo.com"),
            [new(1, 2)], [Producto()], new Dictionary<int, decimal>(), false, ServicioPagoPedidoTienda.MercadoPago).Pedido;

        Assert.Throws<ErrorNegocio>(() => ServicioTransferenciaTienda.Confirmar(pedido, Ahora));
    }

    [Fact]
    public void A_confirmar_no_se_prepara_ni_se_entrega_pero_se_puede_cancelar()
    {
        var pedido = Crear();

        Assert.Throws<ErrorNegocio>(() => ServicioPedidosTienda.Avanzar(pedido, ServicioPedidosTienda.Preparando, Ahora));
        Assert.Throws<ErrorNegocio>(() => ServicioEntregaPedidoTienda.Entregar(pedido, [Producto()], Ahora));
        Assert.Equal([ServicioPedidosTienda.Cancelado], ServicioPagoPedidoTienda.Siguientes(pedido));
    }

    [Fact]
    public void Confirmada_se_prepara_y_se_entrega_por_el_subtotal()
    {
        var pedido = Confirmado(Tienda(aCargoComprador: true));

        ServicioPedidosTienda.Avanzar(pedido, ServicioPedidosTienda.Preparando, Ahora);
        var venta = ServicioEntregaPedidoTienda.Entregar(pedido, [Producto()], Ahora);

        Assert.Equal(ServicioPedidosTienda.Entregado, pedido.Estado);
        Assert.Equal(3000, venta.Total);
    }

    [Fact]
    public void La_gestion_confirmada_queda_pendiente_para_el_abono()
    {
        var confirmado = Confirmado();
        var aConfirmar = Crear();

        Assert.Equal(1000, ServicioGestionTienda.Pendiente([confirmado, aConfirmar]));
        Assert.Equal(0, ServicioGestionTienda.Cobrada([confirmado]));
    }

    [Fact]
    public void Cancelado_antes_de_confirmar_no_genera_deuda()
    {
        var cancelado = Crear();
        ServicioPedidosTienda.CancelarDelLocal(cancelado, Ahora);
        var vencido = Crear();
        ServicioPagoPedidoTienda.Vencer(vencido, Ahora.AddDays(1));

        Assert.Equal(0, ServicioGestionTienda.Pendiente([cancelado, vencido]));
    }

    [Fact]
    public void Cancelar_una_transferencia_confirmada_la_devuelve_sin_deuda()
    {
        var pedido = Confirmado();
        Assert.Throws<ErrorNegocio>(() => ServicioPedidosTienda.CancelarDelLocal(pedido, Ahora));

        ServicioTransferenciaTienda.CancelarConfirmada(pedido, Ahora);

        Assert.Equal(ServicioPedidosTienda.Cancelado, pedido.Estado);
        Assert.Equal(ServicioPagoPedidoTienda.Reembolsado, pedido.EstadoPago);
        Assert.Empty(ServicioPedidosTienda.Reservado([pedido]));
        Assert.Equal(0, ServicioGestionTienda.Pendiente([pedido]));
    }

    [Fact]
    public void Una_transferencia_no_se_reembolsa_por_Mercado_Pago()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioPagoPedidoTienda.CancelarConReembolso(Confirmado(), Ahora));
    }

    [Fact]
    public void Local_eximido_no_genera_deuda()
    {
        Assert.Equal(0, ServicioGestionTienda.Pendiente([Confirmado(Tienda(exento: true))]));
    }

    [Fact]
    public void El_comprador_cancela_mientras_no_se_confirmo()
    {
        var aConfirmar = Crear();
        ServicioPedidosTienda.Cancelar(aConfirmar, Ahora);

        Assert.Equal(ServicioPedidosTienda.Cancelado, aConfirmar.Estado);
        Assert.Throws<ErrorNegocio>(() => ServicioPedidosTienda.Cancelar(Confirmado(), Ahora));
    }

    [Fact]
    public void El_aviso_de_pedido_nuevo_dice_que_hay_que_confirmar_la_transferencia()
    {
        Assert.EndsWith("a confirmar transferencia", ServicioAvisos.Texto(ServicioAvisos.PedidoNuevo, Crear()));
    }
}
