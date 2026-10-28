using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioPedidosTiendaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 28, 12, 0, 0, TimeSpan.FromHours(-3));
    private static readonly DatosComprador Comprador = new("Ana Pérez", "11 4444-5555", "ana@correo.com");
    private static readonly Dictionary<int, decimal> SinReservas = new();

    private static Local Tienda(string estado = ServicioTienda.Activa, int diasAbono = 20, int id = 1) => new()
    {
        IdLocal = id,
        Nombre = "Almagro",
        EstadoAbono = ServicioAbono.AlDia,
        Vence = Ahora.AddDays(diasAbono),
        AliasTienda = "almagro",
        EstadoTienda = estado
    };

    private static Producto Producto(int id = 1, decimal stock = 10, decimal precio = 1500, int local = 1, bool publicado = true, bool sector = false, string? codigo = null) => new()
    {
        Id = id,
        IdLocal = local,
        Codigo = codigo ?? $"779{id}",
        Nombre = $"Producto {id}",
        Cantidad = stock,
        Precio = precio,
        Costo = 1000,
        Publicado = publicado,
        ProductoSector = sector
    };

    private static PedidoCreado Crear(
        IReadOnlyCollection<Producto> productos, ItemPedidoTienda[] items, Local? local = null,
        Dictionary<int, decimal>? reservado = null, bool rigido = false, DatosComprador? datos = null) =>
        ServicioPedidosTienda.Crear(local ?? Tienda(), Ahora, 1, datos ?? Comprador, items, productos, reservado ?? SinReservas, rigido);

    [Fact]
    public void Crear_toma_el_precio_de_lista_del_servidor()
    {
        var pedido = Crear([Producto(precio: 1500)], [new(1, 2)]).Pedido;

        Assert.Equal(1500, pedido.Lineas[0].PrecioUnitario);
        Assert.Equal(3000, pedido.Lineas[0].SubTotal);
        Assert.Equal(3000, pedido.Total);
    }

    [Fact]
    public void El_pedido_congela_el_precio_aunque_cambie_la_lista()
    {
        var producto = Producto(precio: 1500);
        var pedido = Crear([producto], [new(1, 1)]).Pedido;

        producto.Precio = 2000;

        Assert.Equal(1500, pedido.Lineas[0].PrecioUnitario);
        Assert.Equal(1500, pedido.Total);
    }

    [Fact]
    public void Crear_suma_lineas_repetidas_del_mismo_producto()
    {
        var pedido = Crear([Producto()], [new(1, 1), new(1, 2)]).Pedido;

        Assert.Single(pedido.Lineas);
        Assert.Equal(3, pedido.Lineas[0].Cantidad);
    }

    [Fact]
    public void Crear_no_descuenta_stock_del_producto()
    {
        var producto = Producto(stock: 5);

        Crear([producto], [new(1, 2)]);

        Assert.Equal(5, producto.Cantidad);
    }

    [Fact]
    public void El_pedido_nuevo_reserva_stock()
    {
        var pedido = Crear([Producto()], [new(1, 3)]).Pedido;

        var reservado = ServicioPedidosTienda.Reservado([pedido]);

        Assert.Equal(3, reservado[1]);
    }

    [Fact]
    public void Cancelar_libera_la_reserva()
    {
        var pedido = Crear([Producto()], [new(1, 3)]).Pedido;

        ServicioPedidosTienda.Cancelar(pedido, Ahora);

        Assert.Equal(ServicioPedidosTienda.Cancelado, pedido.Estado);
        Assert.Empty(ServicioPedidosTienda.Reservado([pedido]));
    }

    [Fact]
    public void Un_pedido_cancelado_no_se_vuelve_a_cancelar()
    {
        var pedido = Crear([Producto()], [new(1, 1)]).Pedido;
        ServicioPedidosTienda.Cancelar(pedido, Ahora);

        Assert.Throws<ErrorNegocio>(() => ServicioPedidosTienda.Cancelar(pedido, Ahora));
    }

    [Fact]
    public void Stock_rigido_no_deja_pedir_mas_de_lo_disponible()
    {
        Assert.Throws<ErrorNegocio>(() => Crear([Producto(stock: 2)], [new(1, 3)], rigido: true));
    }

    [Fact]
    public void Stock_rigido_descuenta_lo_reservado_por_otros_pedidos()
    {
        var reservado = new Dictionary<int, decimal> { [1] = 4 };

        Assert.Throws<ErrorNegocio>(() => Crear([Producto(stock: 5)], [new(1, 2)], reservado: reservado, rigido: true));
    }

    [Fact]
    public void Sin_stock_rigido_se_puede_pedir_mas_de_lo_que_hay()
    {
        var pedido = Crear([Producto(stock: 1)], [new(1, 5)]).Pedido;

        Assert.Equal(5, pedido.Lineas[0].Cantidad);
    }

    [Fact]
    public void Cotizar_con_stock_rigido_avisa_cuanto_queda()
    {
        var cotizacion = ServicioPedidosTienda.Cotizar(1, [new(1, 5)], [Producto(stock: 3)], new Dictionary<int, decimal> { [1] = 1 }, true);

        Assert.Equal(2, cotizacion.Lineas[0].Maximo);
    }

    [Fact]
    public void Cotizar_marca_el_producto_que_ya_no_esta_publicado()
    {
        var cotizacion = ServicioPedidosTienda.Cotizar(1, [new(1, 1), new(2, 1)], [Producto(1), Producto(2, publicado: false)], SinReservas, false);

        Assert.Equal([2], cotizacion.NoDisponibles);
        Assert.Equal(1500, cotizacion.Total);
    }

    [Theory]
    [InlineData(ServicioTienda.Pausada, 20)]
    [InlineData(ServicioTienda.Inactiva, 20)]
    [InlineData(ServicioTienda.Activa, -1)]
    public void Tienda_pausada_cerrada_o_inactiva_no_recibe_pedidos(string estado, int diasAbono)
    {
        Assert.Throws<ErrorNegocio>(() => Crear([Producto()], [new(1, 1)], local: Tienda(estado, diasAbono)));
    }

    [Fact]
    public void No_se_pide_un_producto_sector_ni_generico()
    {
        Assert.Throws<ErrorNegocio>(() => Crear([Producto(sector: true)], [new(1, 1)]));
        Assert.Throws<ErrorNegocio>(() => Crear([Producto(codigo: "GENERIC-1")], [new(1, 1)]));
    }

    [Fact]
    public void Un_local_no_pide_productos_de_otro()
    {
        Assert.Throws<ErrorNegocio>(() => Crear([Producto(local: 2)], [new(1, 1)], local: Tienda(id: 1)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void La_cantidad_va_de_1_a_99(int cantidad)
    {
        Assert.Throws<ErrorNegocio>(() => Crear([Producto()], [new(1, cantidad)]));
    }

    [Fact]
    public void Carrito_vacio_no_genera_pedido()
    {
        Assert.Throws<ErrorNegocio>(() => Crear([Producto()], []));
    }

    [Theory]
    [InlineData("A", "1144445555", "ana@correo.com")]
    [InlineData("Ana", "123", "ana@correo.com")]
    [InlineData("Ana", "1144445555", "ana-sin-arroba")]
    public void Datos_del_comprador_invalidos_se_rechazan(string nombre, string telefono, string email)
    {
        Assert.Throws<ErrorNegocio>(() => Crear([Producto()], [new(1, 1)], datos: new(nombre, telefono, email)));
    }

    [Fact]
    public void El_pedido_guarda_el_hash_y_no_el_codigo()
    {
        var creado = Crear([Producto()], [new(1, 1)]);

        Assert.True(creado.Codigo.Length >= 40);
        Assert.NotEqual(creado.Codigo, creado.Pedido.HashCodigo);
        Assert.Equal(ServicioPedidosTienda.HashDeCodigo(creado.Codigo), creado.Pedido.HashCodigo);
    }

    [Fact]
    public void Cada_pedido_tiene_un_codigo_distinto()
    {
        Assert.NotEqual(Crear([Producto()], [new(1, 1)]).Codigo, Crear([Producto()], [new(1, 1)]).Codigo);
    }

    [Fact]
    public void Cobrar_en_caja_con_stock_rigido_no_vende_lo_reservado()
    {
        var producto = Producto(stock: 3);

        Assert.Throws<ErrorNegocio>(() => ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [producto],
            Items = [new ItemCarrito { IdProducto = 1, Codigo = producto.Codigo, Nombre = producto.Nombre, Cantidad = 2 }],
            StockRigido = true,
            Reservado = new Dictionary<int, decimal> { [1] = 2 }
        }));
        Assert.Equal(3, producto.Cantidad);
    }

    [Fact]
    public void Cobrar_en_caja_vende_lo_que_no_esta_reservado()
    {
        var producto = Producto(stock: 3);

        ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [producto],
            Items = [new ItemCarrito { IdProducto = 1, Codigo = producto.Codigo, Nombre = producto.Nombre, Cantidad = 1 }],
            StockRigido = true,
            Reservado = new Dictionary<int, decimal> { [1] = 2 }
        });

        Assert.Equal(2, producto.Cantidad);
    }

    [Fact]
    public void El_catalogo_no_muestra_stock_si_todo_esta_reservado()
    {
        var producto = Producto(stock: 2);

        var enTienda = ServicioCatalogoTienda.AProductoTienda(producto, null, null, new Dictionary<int, decimal> { [1] = 2 });

        Assert.False(enTienda.HayStock);
    }
}
