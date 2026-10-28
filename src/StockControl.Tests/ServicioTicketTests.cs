using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioTicketTests
{
    private static readonly DateOnly Hoy = new(2026, 10, 26);

    private static Producto Prod(int id, string nombre, decimal precio, decimal costo) =>
        new() { Id = id, IdLocal = 1, Codigo = $"C{id}", Nombre = nombre, Cantidad = 50, Precio = precio, Costo = costo };

    private static ItemCarrito Item(Producto p, decimal cantidad) =>
        new() { IdProducto = p.Id, Codigo = p.Codigo, Nombre = p.Nombre, Cantidad = cantidad };

    private static Ticket Ticket(
        List<Producto> productos,
        List<ItemCarrito> items,
        int descuento = 0,
        bool alCosto = false,
        Oferta? oferta = null,
        OfertaCombo? combo = null)
    {
        var r = ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = productos,
            Items = items,
            DescuentoPorcentaje = descuento,
            CobrarAlCosto = alCosto,
            Ofertas = oferta == null ? [] : [oferta],
            Combos = combo == null ? [] : [combo],
            Hoy = Hoy
        });
        var t = ServicioTicket.Armar(r.Lineas, r.Total, [new MedioTicket("Efectivo", r.Total)], descuento, DateTimeOffset.UtcNow, 1);
        CierraLaCuenta(t);
        return t;
    }

    private static void CierraLaCuenta(Ticket t)
    {
        Assert.All(t.Lineas, l => Assert.Equal(Math.Round(l.Cantidad * l.Precio, 2), l.SubTotal));
        var ahorros = t.Lineas.Sum(l => l.Ahorro?.Importe ?? 0) + t.Ahorros.Sum(a => a.Importe);
        Assert.Equal(t.Total, t.Lineas.Sum(l => l.SubTotal) - ahorros);
    }

    [Fact]
    public void POS_viejo_pasa_a_POS80()
    {
        Assert.Equal("POS-80", ServicioTicket.NormalizarFormato("POS"));
        Assert.Equal("POS-80", ServicioTicket.NormalizarFormato(""));
        Assert.Equal("POS-58", ServicioTicket.NormalizarFormato("pos-58"));
        Assert.Equal("A4", ServicioTicket.NormalizarFormato("A4"));
        Assert.Equal("POS-80", ServicioTicket.FormatoODefault("CARTA"));
    }

    [Fact]
    public void Formato_invalido_falla()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioTicket.NormalizarFormato("CARTA"));
    }

    [Fact]
    public void Venta_sin_ofertas_muestra_lineas_sin_ahorros()
    {
        var coca = Prod(1, "Coca", 1500, 1000);

        var t = Ticket([coca], [Item(coca, 2)]);

        var linea = Assert.Single(t.Lineas);
        Assert.Equal(1500, linea.Precio);
        Assert.Equal(3000, linea.SubTotal);
        Assert.Null(linea.Ahorro);
        Assert.Empty(t.Ahorros);
        Assert.Equal(3000, t.Total);
        Assert.Equal(2, t.Items);
    }

    [Fact]
    public void Combo_muestra_precio_de_lista_y_ahorro_con_el_nombre_del_combo()
    {
        var coca = Prod(1, "Coca", 2000, 1000);
        var fernet = Prod(2, "Fernet", 10000, 5000);
        var combo = new OfertaCombo
        {
            Id = 1, IdLocal = 1, Nombre = "2 Coca + Fernet", PrecioTotal = 12000, FechaDesde = Hoy, FechaHasta = Hoy,
            Lineas = [new() { IdProducto = 1, Cantidad = 2 }, new() { IdProducto = 2, Cantidad = 1 }]
        };

        var t = Ticket([coca, fernet], [Item(coca, 2), Item(fernet, 1)], combo: combo);

        Assert.Equal([4000m, 10000m], t.Lineas.Select(l => l.SubTotal));
        var ahorro = Assert.Single(t.Ahorros);
        Assert.Equal("Combo 2 Coca + Fernet", ahorro.Descripcion);
        Assert.Equal(2000, ahorro.Importe);
        Assert.Equal(12000, t.Total);
    }

    [Fact]
    public void Llevas_y_pagas_muestra_la_linea_completa_y_el_ahorro_debajo()
    {
        var coca = Prod(1, "Coca", 1000, 600);
        var oferta = new Oferta { Id = 1, IdProducto = 1, Tipo = TipoOferta.LlevaPaga, Lleva = 3, Paga = 2, FechaDesde = Hoy, FechaHasta = Hoy };

        var t = Ticket([coca], [Item(coca, 3)], oferta: oferta);

        var linea = Assert.Single(t.Lineas);
        Assert.Equal(3000, linea.SubTotal);
        Assert.Equal(new AhorroTicket("Oferta 3x2", 1000), linea.Ahorro);
        Assert.Equal(2000, t.Total);
    }

    [Fact]
    public void Precio_por_cantidad_muestra_el_ahorro_debajo()
    {
        var coca = Prod(1, "Coca", 1000, 600);
        var oferta = new Oferta { Id = 1, IdProducto = 1, Tipo = TipoOferta.PrecioDesde, Lleva = 3, PrecioUnidad = 800, FechaDesde = Hoy, FechaHasta = Hoy };

        var t = Ticket([coca], [Item(coca, 4)], oferta: oferta);

        var linea = Assert.Single(t.Lineas);
        Assert.Equal(4000, linea.SubTotal);
        Assert.Equal(800, linea.Ahorro!.Importe);
        Assert.Equal(3200, t.Total);
    }

    [Fact]
    public void Descuento_de_caja_va_al_final_como_linea_de_descuento()
    {
        var coca = Prod(1, "Coca", 1500, 1000);

        var t = Ticket([coca], [Item(coca, 2)], descuento: 10);

        Assert.Equal(1500, t.Lineas[0].Precio);
        Assert.Null(t.Lineas[0].Ahorro);
        Assert.Equal(new AhorroTicket("Descuento 10 %", 100), Assert.Single(t.Ahorros));
        Assert.Equal(2900, t.Total);
    }

    [Fact]
    public void Oferta_y_descuento_de_caja_se_ven_por_separado()
    {
        var coca = Prod(1, "Coca", 1000, 500);
        var oferta = new Oferta { Id = 1, IdProducto = 1, Tipo = TipoOferta.Descuento, Porcentaje = 20, FechaDesde = Hoy, FechaHasta = Hoy };

        var t = Ticket([coca], [Item(coca, 1)], descuento: 10, oferta: oferta);

        Assert.Equal(200, t.Lineas[0].Ahorro!.Importe);
        Assert.Equal(30, Assert.Single(t.Ahorros).Importe);
        Assert.Equal(770, t.Total);
    }

    [Fact]
    public void Al_costo_muestra_el_precio_cobrado_sin_linea_de_ahorro()
    {
        var coca = Prod(1, "Coca", 1500, 1000);

        var t = Ticket([coca], [Item(coca, 2)], alCosto: true);

        Assert.Equal(1000, t.Lineas[0].Precio);
        Assert.Null(t.Lineas[0].Ahorro);
        Assert.Empty(t.Ahorros);
        Assert.Equal(2000, t.Total);
    }

    [Fact]
    public void Venta_anterior_sin_precio_base_se_ve_como_estaba()
    {
        var vieja = new InformeVentaDetalle { Codigo = "779", Nombre = "Coca", Cantidad = 2, Precio = 1400, SubTotal = 2800, DescripcionOferta = "Combo" };

        var t = ServicioTicket.Armar([vieja], 2800, [], 0, DateTimeOffset.UtcNow, 1);

        var linea = Assert.Single(t.Lineas);
        Assert.Equal(1400, linea.Precio);
        Assert.Equal(2800, linea.SubTotal);
        Assert.Equal("Combo", linea.Nota);
        Assert.Empty(t.Ahorros);
    }

    [Fact]
    public void Cuenta_pendiente_lleva_el_cliente_como_medio()
    {
        var cuenta = new CuentaPendiente
        {
            Importe = 1500,
            Saldo = 1500,
            NroCaja = 2,
            Lineas = [new LineaCuenta { Codigo = "779", Nombre = "Coca", Cantidad = 1, Precio = 1500, SubTotal = 1500, PrecioBase = 1500 }]
        };

        var t = ServicioTicket.DeCuenta(cuenta, "María");

        Assert.Equal(new MedioTicket("A cuenta de María", 1500), Assert.Single(t.Medios));
        Assert.Equal(2, t.NroCaja);
    }

    [Fact]
    public void MediosDe_varios_pagos_con_su_importe()
    {
        var medios = ServicioTicket.MediosDe(
            [
                new PagoVenta { DescripcionMetodoPago = "Efectivo", Importe = 60 },
                new PagoVenta { DescripcionMetodoPago = "Transferencia", Importe = 40 }
            ],
            "Efectivo, Transferencia",
            100);

        Assert.Equal([new MedioTicket("Efectivo", 60), new MedioTicket("Transferencia", 40)], medios);
    }

    [Fact]
    public void MediosDe_sin_pagos_usa_el_metodo_con_el_total()
    {
        Assert.Equal([new MedioTicket("Efectivo", 500)], ServicioTicket.MediosDe([], "Efectivo", 500));
    }

    [Fact]
    public void Armar_sin_items_falla()
    {
        Assert.Throws<ErrorNegocio>(() =>
            ServicioTicket.Armar([], 0, [], 0, DateTimeOffset.UtcNow, 1));
    }
}
