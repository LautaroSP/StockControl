using StockControl.Dominio;

namespace StockControl.Tests;

public class PanelPedidosTiendaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 28, 12, 0, 0, TimeSpan.FromHours(-3));

    private static Producto Producto(int id = 1, decimal stock = 10, int local = 1, bool sector = false) => new()
    {
        Id = id,
        IdLocal = local,
        Codigo = $"779{id}",
        Nombre = $"Producto {id}",
        Cantidad = stock,
        Precio = 1500,
        Costo = 1000,
        Publicado = true,
        ProductoSector = sector
    };

    private static PedidoTienda Pedido(string estado = ServicioPedidosTienda.Nuevo, decimal cantidad = 2, decimal precio = 1500) => new()
    {
        Id = 7,
        IdLocal = 1,
        Numero = 3,
        Estado = estado,
        Nombre = "Ana Pérez",
        Subtotal = cantidad * precio,
        Total = cantidad * precio,
        Lineas =
        [
            new LineaPedidoTienda
            {
                IdLocal = 1, IdProducto = 1, Codigo = "7791", Nombre = "Producto 1",
                Cantidad = cantidad, PrecioUnitario = precio, SubTotal = cantidad * precio
            }
        ]
    };

    [Theory]
    [InlineData(ServicioPedidosTienda.Nuevo, ServicioPedidosTienda.Preparando)]
    [InlineData(ServicioPedidosTienda.Nuevo, ServicioPedidosTienda.Listo)]
    [InlineData(ServicioPedidosTienda.Preparando, ServicioPedidosTienda.Listo)]
    public void Avanzar_acepta_las_transiciones_validas(string desde, string hacia)
    {
        var pedido = Pedido(desde);

        ServicioPedidosTienda.Avanzar(pedido, hacia, Ahora);

        Assert.Equal(hacia, pedido.Estado);
        Assert.Equal(Ahora, pedido.Actualizado);
    }

    [Theory]
    [InlineData(ServicioPedidosTienda.Preparando, ServicioPedidosTienda.Preparando)]
    [InlineData(ServicioPedidosTienda.Listo, ServicioPedidosTienda.Preparando)]
    [InlineData(ServicioPedidosTienda.Entregado, ServicioPedidosTienda.Listo)]
    [InlineData(ServicioPedidosTienda.Cancelado, ServicioPedidosTienda.Preparando)]
    public void Avanzar_rechaza_las_transiciones_invalidas(string desde, string hacia)
    {
        var pedido = Pedido(desde);

        Assert.Throws<ErrorNegocio>(() => ServicioPedidosTienda.Avanzar(pedido, hacia, Ahora));
        Assert.Equal(desde, pedido.Estado);
    }

    [Theory]
    [InlineData(ServicioPedidosTienda.Entregado)]
    [InlineData(ServicioPedidosTienda.Cancelado)]
    [InlineData("pagado")]
    public void Avanzar_no_entrega_ni_cancela_ni_inventa_estados(string estado)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioPedidosTienda.Avanzar(Pedido(), estado, Ahora));
    }

    [Theory]
    [InlineData(ServicioPedidosTienda.Nuevo)]
    [InlineData(ServicioPedidosTienda.Preparando)]
    [InlineData(ServicioPedidosTienda.Listo)]
    public void El_pedido_abierto_reserva_stock(string estado)
    {
        Assert.Equal(2, ServicioPedidosTienda.Reservado([Pedido(estado)])[1]);
    }

    [Theory]
    [InlineData(ServicioPedidosTienda.Preparando)]
    [InlineData(ServicioPedidosTienda.Listo)]
    public void El_local_cancela_un_pedido_en_preparacion_y_libera_el_stock(string estado)
    {
        var pedido = Pedido(estado);

        ServicioPedidosTienda.CancelarDelLocal(pedido, Ahora);

        Assert.Equal(ServicioPedidosTienda.Cancelado, pedido.Estado);
        Assert.Equal(Ahora, pedido.Cancelado);
        Assert.Empty(ServicioPedidosTienda.Reservado([pedido]));
    }

    [Theory]
    [InlineData(ServicioPedidosTienda.Entregado)]
    [InlineData(ServicioPedidosTienda.Cancelado)]
    public void El_local_no_cancela_un_pedido_cerrado(string estado)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioPedidosTienda.CancelarDelLocal(Pedido(estado), Ahora));
    }

    [Fact]
    public void El_comprador_no_cancela_un_pedido_en_preparacion()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioPedidosTienda.Cancelar(Pedido(ServicioPedidosTienda.Preparando), Ahora));
    }

    [Fact]
    public void Entregar_registra_la_venta_con_el_precio_congelado()
    {
        var producto = Producto();
        producto.Precio = 9999;

        var venta = ServicioEntregaPedidoTienda.Entregar(Pedido(precio: 1500), [producto], Ahora);

        var linea = Assert.Single(venta.Lineas);
        Assert.Equal(1500, linea.Precio);
        Assert.Equal(3000, linea.SubTotal);
        Assert.Equal(1000, linea.Costo);
        Assert.Equal(1, linea.IdProducto);
        Assert.Equal(3000, venta.Total);
    }

    [Fact]
    public void Entregar_descuenta_el_stock_y_libera_la_reserva()
    {
        var producto = Producto(stock: 10);
        var pedido = Pedido(ServicioPedidosTienda.Listo);

        ServicioEntregaPedidoTienda.Entregar(pedido, [producto], Ahora);

        Assert.Equal(8, producto.Cantidad);
        Assert.Equal(ServicioPedidosTienda.Entregado, pedido.Estado);
        Assert.Equal(Ahora, pedido.Entregado);
        Assert.Empty(ServicioPedidosTienda.Reservado([pedido]));
    }

    [Fact]
    public void Entregar_con_menos_stock_que_lo_pedido_deja_el_stock_en_cero()
    {
        var producto = Producto(stock: 1);

        ServicioEntregaPedidoTienda.Entregar(Pedido(cantidad: 3), [producto], Ahora);

        Assert.Equal(0, producto.Cantidad);
    }

    [Fact]
    public void Entregar_un_producto_borrado_igual_registra_la_linea()
    {
        var venta = ServicioEntregaPedidoTienda.Entregar(Pedido(), [], Ahora);

        var linea = Assert.Single(venta.Lineas);
        Assert.Equal(0, linea.IdProducto);
        Assert.Equal("Producto 1", linea.Nombre);
        Assert.Null(linea.Costo);
    }

    [Fact]
    public void Entregar_no_toca_productos_de_otro_local()
    {
        var ajeno = Producto(local: 2, stock: 10);

        var venta = ServicioEntregaPedidoTienda.Entregar(Pedido(), [ajeno], Ahora);

        Assert.Equal(10, ajeno.Cantidad);
        Assert.Equal(0, venta.Lineas[0].IdProducto);
    }

    [Theory]
    [InlineData(ServicioPedidosTienda.Entregado)]
    [InlineData(ServicioPedidosTienda.Cancelado)]
    public void Un_pedido_cerrado_no_se_entrega(string estado)
    {
        var producto = Producto(stock: 10);

        Assert.Throws<ErrorNegocio>(() => ServicioEntregaPedidoTienda.Entregar(Pedido(estado), [producto], Ahora));
        Assert.Equal(10, producto.Cantidad);
    }

    [Theory]
    [InlineData(Roles.Dueno, false, true)]
    [InlineData(Roles.Socio, false, true)]
    [InlineData(Roles.Empleado, false, false)]
    [InlineData(Roles.Empleado, true, true)]
    public void Ver_pedidos_de_la_tienda_depende_del_rol(string rol, bool habilitado, bool puede)
    {
        Assert.Equal(puede, Permisos.PuedeGestionarPedidosTienda(rol, habilitado));
    }
}
