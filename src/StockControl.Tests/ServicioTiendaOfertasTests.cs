using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioTiendaOfertasTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 28, 12, 0, 0, TimeSpan.FromHours(-3));
    private static readonly DateOnly Hoy = new(2026, 10, 28);
    private static readonly DatosComprador Comprador = new("Ana Pérez", "11 4444-5555", "ana@correo.com");
    private static readonly Dictionary<int, decimal> SinReservas = new();

    private static Local Tienda() => new()
    {
        IdLocal = 1, Nombre = "Almagro", EstadoAbono = ServicioAbono.AlDia, Vence = Ahora.AddDays(20),
        AliasTienda = "almagro", EstadoTienda = ServicioTienda.Activa
    };

    private static Producto Producto(int id = 1, decimal precio = 1000, decimal stock = 50, string? codigo = null, bool sector = false) => new()
    {
        Id = id, IdLocal = 1, Codigo = codigo ?? $"779{id}", Nombre = $"Producto {id}", Cantidad = stock,
        Precio = precio, Costo = 500, Publicado = true, ProductoSector = sector
    };

    private static Oferta Oferta(string tipo, bool enTienda = true, int porcentaje = 0, int lleva = 0, int paga = 0, decimal precioUnidad = 0,
        int idProducto = 1, DateOnly? desde = null, DateOnly? hasta = null) => new()
    {
        Id = 1, IdProducto = idProducto, Tipo = tipo, Porcentaje = porcentaje, Lleva = lleva, Paga = paga, PrecioUnidad = precioUnidad,
        FechaDesde = desde ?? Hoy, FechaHasta = hasta ?? Hoy, EnTienda = enTienda
    };

    private static CotizacionTienda Cotizar(Producto producto, int cantidad, Oferta? oferta, bool rigido = false) =>
        ServicioPedidosTienda.Cotizar(1, [new(producto.Id, cantidad)], [producto], SinReservas, rigido, oferta == null ? [] : [oferta], Hoy);

    [Fact]
    public void Tienda_aplica_oferta_marcada_en_tienda()
    {
        var oferta = Oferta(TipoOferta.Descuento, porcentaje: 10);

        Assert.Same(oferta, ServicioOfertas.AplicableEnTienda([oferta], 1, Hoy));
    }

    [Fact]
    public void Tienda_ignora_oferta_sin_en_tienda()
    {
        var oferta = Oferta(TipoOferta.Descuento, enTienda: false, porcentaje: 10);

        Assert.Null(ServicioOfertas.AplicableEnTienda([oferta], 1, Hoy));
    }

    [Fact]
    public void Tienda_ignora_oferta_vencida_o_futura()
    {
        var vencida = Oferta(TipoOferta.Descuento, porcentaje: 10, desde: Hoy.AddDays(-5), hasta: Hoy.AddDays(-1));
        var futura = Oferta(TipoOferta.Descuento, porcentaje: 10, desde: Hoy.AddDays(1), hasta: Hoy.AddDays(5));

        Assert.Null(ServicioOfertas.AplicableEnTienda([vencida], 1, Hoy));
        Assert.Null(ServicioOfertas.AplicableEnTienda([futura], 1, Hoy));
    }

    [Fact]
    public void Tienda_no_aplica_opciones_de_caja_al_costo_ni_descuento()
    {
        var oferta = Oferta(TipoOferta.Descuento, porcentaje: 10);
        oferta.AnulaConAlCosto = true;
        oferta.AnulaConDescuentoCaja = true;
        oferta.PermiteNoAplicarEnCaja = true;

        Assert.Same(oferta, ServicioOfertas.AplicableEnTienda([oferta], 1, Hoy));
    }

    [Fact]
    public void Cotizar_tienda_descuento_calcula_precio_y_ahorro()
    {
        var linea = Cotizar(Producto(precio: 1000), 2, Oferta(TipoOferta.Descuento, porcentaje: 20)).Lineas[0];

        Assert.Equal(800, linea.PrecioUnitario);
        Assert.Equal(1600, linea.SubTotal);
        Assert.Equal(1000, linea.PrecioLista);
        Assert.Equal(400, linea.Ahorro);
        Assert.Equal("20 % off", linea.Oferta);
    }

    [Fact]
    public void Cotizar_tienda_lleva_paga_cobra_unidades_correctas()
    {
        var linea = Cotizar(Producto(precio: 1000), 3, Oferta(TipoOferta.LlevaPaga, lleva: 3, paga: 2)).Lineas[0];

        Assert.Equal(2000, linea.SubTotal);
        Assert.Equal(1000, linea.Ahorro);
        Assert.Equal("3x2", linea.Oferta);
    }

    [Fact]
    public void Cotizar_tienda_precio_desde_y_exacto_segun_cantidad()
    {
        var desde = Oferta(TipoOferta.PrecioDesde, lleva: 3, precioUnidad: 700);
        var exacto = Oferta(TipoOferta.PrecioExacto, lleva: 3, precioUnidad: 700);

        Assert.Equal(2800, Cotizar(Producto(), 4, desde).Lineas[0].SubTotal);
        Assert.Equal(2000, Cotizar(Producto(), 2, desde).Lineas[0].SubTotal);
        Assert.Equal(2100, Cotizar(Producto(), 3, exacto).Lineas[0].SubTotal);
        Assert.Equal(4000, Cotizar(Producto(), 4, exacto).Lineas[0].SubTotal);
    }

    [Fact]
    public void Cotizar_tienda_sin_oferta_no_tiene_ahorro()
    {
        var sinMarca = Oferta(TipoOferta.Descuento, enTienda: false, porcentaje: 50);
        var linea = Cotizar(Producto(precio: 1000), 2, sinMarca).Lineas[0];

        Assert.Equal(2000, linea.SubTotal);
        Assert.Equal(0, linea.Ahorro);
        Assert.Equal("", linea.Oferta);
    }

    [Fact]
    public void Cotizar_tienda_producto_sector_o_generico_sin_oferta()
    {
        var oferta = Oferta(TipoOferta.Descuento, porcentaje: 10);
        var sector = Producto(sector: true);
        var generico = Producto(codigo: "GENERIC-1");

        Assert.Single(Cotizar(sector, 1, oferta).NoDisponibles);
        Assert.Single(Cotizar(generico, 1, oferta).NoDisponibles);
    }

    [Fact]
    public void Cotizar_tienda_stock_rigido_sigue_limitando_con_oferta()
    {
        var linea = Cotizar(Producto(stock: 2), 5, Oferta(TipoOferta.Descuento, porcentaje: 10), rigido: true).Lineas[0];

        Assert.Equal(2, linea.Maximo);
    }

    [Fact]
    public void Pedido_tienda_guarda_precio_lista_oferta_y_ahorro()
    {
        var creado = ServicioPedidosTienda.Crear(
            Tienda(), Ahora, 1, Comprador, [new(1, 2)], [Producto(precio: 1000)], SinReservas, false,
            ofertas: [Oferta(TipoOferta.Descuento, porcentaje: 25)]);

        var linea = creado.Pedido.Lineas[0];
        Assert.Equal(1000, linea.PrecioLista);
        Assert.Equal(750, linea.PrecioUnitario);
        Assert.Equal("25 % off", linea.Oferta);
        Assert.Equal(500, linea.Ahorro);
        Assert.Equal(1500, creado.Pedido.Total);
    }

    [Fact]
    public void Pedido_tienda_no_cambia_si_la_oferta_se_edita_despues()
    {
        var oferta = Oferta(TipoOferta.Descuento, porcentaje: 25);
        var pedido = ServicioPedidosTienda.Crear(
            Tienda(), Ahora, 1, Comprador, [new(1, 1)], [Producto(precio: 1000)], SinReservas, false, ofertas: [oferta]).Pedido;

        oferta.Porcentaje = 90;

        Assert.Equal(750, pedido.Lineas[0].SubTotal);
        Assert.Equal(750, pedido.Total);
    }

    [Fact]
    public void Entregar_pedido_con_oferta_vende_con_el_precio_de_lista_como_base()
    {
        var producto = Producto(precio: 1000);
        var pedido = ServicioPedidosTienda.Crear(
            Tienda(), Ahora, 1, Comprador, [new(1, 2)], [producto], SinReservas, false,
            ofertas: [Oferta(TipoOferta.Descuento, porcentaje: 20)]).Pedido;

        var venta = ServicioEntregaPedidoTienda.Entregar(pedido, [producto], Ahora);

        Assert.Equal(1600, venta.Total);
        Assert.Equal(1000, venta.Lineas[0].PrecioBase);
    }

    [Fact]
    public void Descripcion_producto_admite_vacio_y_rechaza_mas_de_500()
    {
        Assert.Null(ServicioCatalogoTienda.NormalizarDescripcion(null));
        Assert.Null(ServicioCatalogoTienda.NormalizarDescripcion("   "));
        Assert.Equal("Gaseosa 1,5 L", ServicioCatalogoTienda.NormalizarDescripcion("  Gaseosa 1,5 L "));
        Assert.NotNull(ServicioCatalogoTienda.NormalizarDescripcion(new string('a', 500)));
        Assert.Throws<ErrorNegocio>(() => ServicioCatalogoTienda.NormalizarDescripcion(new string('a', 501)));
    }

    [Fact]
    public void Ficha_trae_la_descripcion_del_producto()
    {
        var producto = Producto();
        producto.Descripcion = "Botella retornable";

        Assert.Equal("Botella retornable", ServicioCatalogoTienda.AProductoTienda(producto, null).Descripcion);
    }

    [Fact]
    public void Orden_tienda_acepta_solo_nombre_o_precio()
    {
        Assert.Equal("precio", ServicioCatalogoTienda.NormalizarOrden("PRECIO"));
        Assert.Equal("nombre", ServicioCatalogoTienda.NormalizarOrden("nombre"));
        Assert.Equal("nombre", ServicioCatalogoTienda.NormalizarOrden("Costo; DROP TABLE"));
        Assert.Equal("nombre", ServicioCatalogoTienda.NormalizarOrden(null));
    }

    [Fact]
    public void Orden_por_precio_usa_precio_con_oferta()
    {
        var caro = Producto(1, precio: 1000);
        var barato = Producto(2, precio: 600);
        var ofertas = new[] { Oferta(TipoOferta.Descuento, porcentaje: 50, idProducto: 1) };

        var ids = ServicioCatalogoTienda.Ordenar(new[] { barato, caro }.AsQueryable(), ofertas.AsQueryable(), "precio", "asc")
            .Select(p => p.Id).ToList();

        Assert.Equal([1, 2], ids);
    }

    [Fact]
    public void Orden_por_precio_descendente_invierte_el_orden()
    {
        var ids = ServicioCatalogoTienda.Ordenar(
            new[] { Producto(1, precio: 100), Producto(2, precio: 300) }.AsQueryable(), Array.Empty<Oferta>().AsQueryable(), "precio", "desc")
            .Select(p => p.Id).ToList();

        Assert.Equal([2, 1], ids);
    }

    [Fact]
    public void Ficha_y_catalogo_muestran_precio_tachado_con_oferta_de_tienda()
    {
        var oferta = Oferta(TipoOferta.Descuento, porcentaje: 10);

        var tarjeta = ServicioCatalogoTienda.AProductoTienda(Producto(precio: 1000), null, ofertasTienda: [oferta], hoy: Hoy);

        Assert.Equal(900, tarjeta.Precio);
        Assert.Equal(1000, tarjeta.PrecioLista);
        Assert.Equal(100, tarjeta.Ahorro);
    }

    [Fact]
    public void Open_graph_escapa_nombre_y_omite_imagen_sin_logo()
    {
        var html = ServicioOpenGraphTienda.Html("Don <Pepe> & \"Hijos\"", "https://x.com/tienda/pepe", null);

        Assert.Contains("Don &lt;Pepe&gt; &amp; &quot;Hijos&quot;", html);
        Assert.DoesNotContain("<Pepe>", html);
        Assert.DoesNotContain("og:image", html);
        Assert.Contains("og:title", html);
    }

    [Fact]
    public void Open_graph_incluye_logo_cuando_hay()
    {
        var html = ServicioOpenGraphTienda.Html("Pepe", "https://x.com/tienda/pepe", "https://x.com/api/tienda/pepe/logo");

        Assert.Contains("og:image\" content=\"https://x.com/api/tienda/pepe/logo", html);
    }

    [Fact]
    public void Aviso_dueno_solo_con_motivo_abono()
    {
        Assert.NotNull(ServicioTienda.AvisoDueno("abono"));
        Assert.Null(ServicioTienda.AvisoDueno("gestion"));
        Assert.Null(ServicioTienda.AvisoDueno(null));
    }

    [Fact]
    public void Oferta_en_tienda_solo_la_edita_el_dueno()
    {
        Assert.True(Permisos.PuedeAdministrarOfertas(Roles.Dueno));
        Assert.False(Permisos.PuedeAdministrarOfertas(Roles.Empleado));
    }
}
