using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioDescuentoMedioTests
{
    private static readonly DateOnly Hoy = new(2026, 11, 20);

    private static Producto Coca() => new()
    {
        Id = 1,
        IdLocal = 1,
        Codigo = "779",
        Nombre = "Coca",
        Cantidad = 10,
        Precio = 1500,
        Costo = 1000
    };

    private static ResultadoCobro Cobrar(int descuento = 0, Oferta? oferta = null)
    {
        return ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [Coca()],
            Items = [new ItemCarrito { IdProducto = 1, Codigo = "779", Nombre = "Coca", Cantidad = 1, PrecioUnitario = 1500 }],
            DescuentoPorcentaje = descuento,
            Ofertas = oferta == null ? [] : [oferta],
            Hoy = Hoy,
            MetodoPago = "Efectivo"
        });
    }

    [Fact]
    public void Un_medio_con_10_baja_el_total_y_el_pago_tiene_que_ser_ese_importe()
    {
        var r = Cobrar();
        ServicioDescuentoMedio.Aplicar(r, [(1, 10)]);

        Assert.Equal(150, r.DescuentoMedio);
        Assert.Equal(10, r.DescuentoMedioPorcentaje);
        Assert.Equal(1350, r.Total);
        ServicioPagos.Validar(r.Total, [new PagoSolicitado(1, 1350)]);
        Assert.Throws<ErrorNegocio>(() => ServicioPagos.Validar(r.Total, [new PagoSolicitado(1, 1500)]));
    }

    [Fact]
    public void Dos_medios_no_descuentan()
    {
        var r = Cobrar();
        ServicioDescuentoMedio.Aplicar(r, [(1, 10), (2, 10)]);

        Assert.Equal(0, r.DescuentoMedio);
        Assert.Equal(1500, r.Total);
    }

    [Fact]
    public void Cero_por_ciento_no_cambia_el_total()
    {
        var r = Cobrar();
        ServicioDescuentoMedio.Aplicar(r, [(1, 0)]);

        Assert.Equal(0, r.DescuentoMedio);
        Assert.Equal(1500, r.Total);
    }

    [Fact]
    public void El_porcentaje_corre_despues_de_la_oferta_y_del_descuento_de_caja()
    {
        var oferta = new Oferta
        {
            Id = 1,
            IdProducto = 1,
            Tipo = TipoOferta.Descuento,
            Porcentaje = 20,
            FechaDesde = Hoy,
            FechaHasta = Hoy
        };
        var r = Cobrar(descuento: 50, oferta: oferta);
        Assert.Equal(1100, r.Total);

        ServicioDescuentoMedio.Aplicar(r, [(1, 10)]);

        Assert.Equal(110, r.DescuentoMedio);
        Assert.Equal(990, r.Total);
    }

    [Fact]
    public void El_ticket_guarda_la_linea_con_el_importe_cobrado()
    {
        var r = Cobrar();
        ServicioDescuentoMedio.Aplicar(r, [(1, 10)]);
        var venta = new InformeVenta
        {
            Total = r.Total,
            Fecha = DateTimeOffset.UtcNow,
            NroCaja = 1,
            Detalles = r.Lineas.Select(l => InformeVentaDetalle.Desde(l, 1)).ToList(),
            Pagos = [new PagoVenta { DescripcionMetodoPago = "Efectivo", Importe = r.Total }],
            DescuentoMedio = r.DescuentoMedio,
            DescuentoMedioPorcentaje = r.DescuentoMedioPorcentaje
        };

        var ticket = ServicioTicket.DeVenta(venta);

        Assert.Contains(ticket.Ahorros, a => a.Descripcion == "Efectivo 10 %" && a.Importe == 150);
        Assert.Equal(1350, ticket.Total);
        var ahorros = ticket.Lineas.Sum(l => l.Ahorro?.Importe ?? 0) + ticket.Ahorros.Sum(a => a.Importe);
        Assert.Equal(ticket.Total, ticket.Lineas.Sum(l => l.SubTotal) - ahorros);
    }

    [Fact]
    public void Un_pago_de_cuenta_no_recibe_el_descuento()
    {
        var r = new ResultadoCobro { Total = 1000 };
        r.Lineas.Add(new LineaVenta { Codigo = "CUENTA-1", Nombre = "Pago", Cantidad = 1, Precio = 1000, SubTotal = 1000 });

        ServicioDescuentoMedio.Aplicar(r, [(1, 10)]);

        Assert.Equal(0, r.DescuentoMedio);
        Assert.Equal(1000, r.Total);
    }

    [Fact]
    public void Porcentaje_fuera_de_0_a_99_se_rechaza()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioDescuentoMedio.Exigir(100));
        Assert.Throws<ErrorNegocio>(() => ServicioDescuentoMedio.Exigir(-1));
    }
}
