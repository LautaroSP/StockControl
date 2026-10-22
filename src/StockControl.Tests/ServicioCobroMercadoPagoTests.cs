using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCobroMercadoPagoTests
{
    private static Producto Comun() => new()
    {
        Id = 1,
        IdLocal = 1,
        Codigo = "779",
        Nombre = "Coca",
        Cantidad = 10,
        Precio = 1500,
        Costo = 1000
    };

    private static PedidoCobro Pedido(Producto prod) => new()
    {
        ProductosDelLocal = [prod],
        Items = [new ItemCarrito { IdProducto = 1, Codigo = "779", Nombre = "Coca", Cantidad = 2, PrecioUnitario = 1500 }],
        MetodoPago = "Mercado Pago"
    };

    [Fact]
    public void Esperando_no_descuenta_stock()
    {
        var prod = Comun();
        var cobro = new CobroMercadoPago { Estado = EstadoCobroMercadoPago.Esperando, Modo = IntegracionPago.QrPantalla };
        ServicioCobroMercadoPago.ValidarApertura(true, cobro.Modo, true, false, false);

        Assert.Equal(EstadoCobroMercadoPago.Esperando, cobro.Estado);
        Assert.Equal(10, prod.Cantidad);
    }

    [Fact]
    public void Pagado_descuenta_una_sola_vez()
    {
        var prod = Comun();
        var cobro = new CobroMercadoPago { Estado = EstadoCobroMercadoPago.Esperando };
        var pedido = Pedido(prod);

        if (ServicioCobroMercadoPago.IntentarMarcarPagado(cobro))
            ServicioCobro.Cobrar(pedido);
        if (ServicioCobroMercadoPago.IntentarMarcarPagado(cobro))
            ServicioCobro.Cobrar(pedido);

        Assert.Equal(8, prod.Cantidad);
        Assert.Equal(EstadoCobroMercadoPago.Pagado, cobro.Estado);
    }

    [Fact]
    public void Cancelar_no_toca_el_stock()
    {
        var prod = Comun();
        var cobro = new CobroMercadoPago { Estado = EstadoCobroMercadoPago.Esperando };
        ServicioCobroMercadoPago.Cancelar(cobro);

        Assert.Equal(EstadoCobroMercadoPago.Cancelado, cobro.Estado);
        Assert.Equal(10, prod.Cantidad);
    }

    [Fact]
    public void Qr_impreso_rechaza_un_segundo_cobro_abierto()
    {
        var ex = Assert.Throws<ErrorNegocio>(() =>
            ServicioCobroMercadoPago.ValidarApertura(true, IntegracionPago.QrImpreso, true, false, true));

        Assert.Equal("Este puesto ya tiene un cobro abierto en el QR impreso.", ex.Message);
    }

    [Fact]
    public void Sin_cuenta_no_inicia_la_orden()
    {
        var ex = Assert.Throws<ErrorNegocio>(() =>
            ServicioCobroMercadoPago.ValidarApertura(false, IntegracionPago.QrPantalla, true, false, false));

        Assert.Equal("Este local no tiene Mercado Pago conectado.", ex.Message);
    }

    [Fact]
    public void Medio_manual_no_espera()
    {
        Assert.False(IntegracionPago.EsMercadoPago(IntegracionPago.Manual));
        Assert.False(IntegracionPago.EsMercadoPago(""));
    }

    [Fact]
    public void Mixto_cobra_en_Mercado_Pago_solo_esa_parte()
    {
        var pagos = new List<PagoConIntegracion>
        {
            new(1, IntegracionPago.Manual, 1000),
            new(2, IntegracionPago.QrPantalla, 500)
        };

        Assert.Equal(IntegracionPago.QrPantalla, ServicioCobroMercadoPago.ModoDe(pagos, 1500));
        Assert.Equal(500, ServicioCobroMercadoPago.ImporteDeLaOrden(pagos));
    }

    [Fact]
    public void Dos_medios_de_Mercado_Pago_no_entran_en_el_mismo_cobro()
    {
        var pagos = new List<PagoConIntegracion>
        {
            new(1, IntegracionPago.QrPantalla, 1000),
            new(2, IntegracionPago.Point, 500)
        };

        Assert.Throws<ErrorNegocio>(() => ServicioCobroMercadoPago.ModoDe(pagos, 1500));
    }

    [Fact]
    public void Qr_rechaza_un_importe_menor_a_quince()
    {
        var ex = Assert.Throws<ErrorNegocio>(() =>
            ServicioCobroMercadoPago.ValidarImporteQr(IntegracionPago.QrPantalla, 14.99m));

        Assert.Equal("El QR de Mercado Pago pide al menos $ 15.", ex.Message);
        ServicioCobroMercadoPago.ValidarImporteQr(IntegracionPago.QrImpreso, 15m);
        ServicioCobroMercadoPago.ValidarImporteQr(IntegracionPago.Point, 1m);
    }

    [Fact]
    public void Orden_procesada_es_pagada()
    {
        Assert.Equal(EstadoCobroMercadoPago.Pagado, ServicioCobroMercadoPago.EstadoDesdeOrden("processed"));
        Assert.Equal(EstadoCobroMercadoPago.Vencido, ServicioCobroMercadoPago.EstadoDesdeOrden("expired"));
        Assert.Equal(EstadoCobroMercadoPago.Esperando, ServicioCobroMercadoPago.EstadoDesdeOrden("created"));
    }
}
