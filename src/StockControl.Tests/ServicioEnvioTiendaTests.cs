using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioEnvioTiendaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 11, 1, 12, 0, 0, TimeSpan.FromHours(-3));

    private static Local Local(decimal? precio = 100, decimal? minimo = null, decimal? radio = null, decimal? gratisDesde = null, bool gratisSiempre = false, bool ubicado = true)
    {
        var local = new Local
        {
            IdLocal = 1,
            Nombre = "Almagro",
            EstadoAbono = ServicioAbono.AlDia,
            Vence = Ahora.AddDays(20),
            AliasTienda = "almagro",
            EstadoTienda = ServicioTienda.Activa,
            HaceEnvios = true,
            PrecioEnvioKm = precio,
            EnvioMinimo = minimo,
            RadioEnvioKm = radio,
            EnvioGratisDesde = gratisDesde,
            EnvioGratisSiempre = gratisSiempre
        };
        if (ubicado)
        {
            local.Latitud = 0;
            local.Longitud = 0;
        }
        return local;
    }

    [Fact]
    public void Cotizar_cobra_el_precio_por_km()
    {
        var envio = ServicioEnvioTienda.Cotizar(Local(), subtotal: 1500, latitud: 0, longitud: 1, kmRecorrido: 2.5m);

        Assert.True(envio.Disponible);
        Assert.Equal(ServicioEnvioTienda.Recorrido, envio.Origen);
        Assert.Equal(2.5m, envio.DistanciaKm);
        Assert.Equal(250m, envio.Importe);
    }

    [Fact]
    public void Cotizar_no_baja_del_minimo()
    {
        var envio = ServicioEnvioTienda.Cotizar(Local(precio: 10, minimo: 80), 1500, 0, 1, kmRecorrido: 2);

        Assert.Equal(80m, envio.Importe);
    }

    [Fact]
    public void Fuera_del_radio_solo_permite_retiro()
    {
        var envio = ServicioEnvioTienda.Cotizar(Local(radio: 5), 1500, 0, 1, kmRecorrido: 6);

        Assert.False(envio.Disponible);
        Assert.Equal(0, envio.Importe);
        Assert.Contains("retirar", envio.Motivo);
    }

    [Fact]
    public void Gratis_desde_un_monto_deja_el_envio_en_cero()
    {
        var envio = ServicioEnvioTienda.Cotizar(Local(minimo: 50, gratisDesde: 1000), subtotal: 1000, 0, 1, kmRecorrido: 3);

        Assert.True(envio.Disponible);
        Assert.Equal(0, envio.Importe);
    }

    [Fact]
    public void Gratis_siempre_no_pide_precio_y_el_importe_es_cero()
    {
        var local = Local(precio: null, minimo: 80, gratisSiempre: true);

        ServicioEnvioTienda.Asignar(local, gratisSiempre: true, precioKm: null, minimo: 80, radioKm: null, gratisDesde: null);
        var envio = ServicioEnvioTienda.Cotizar(local, 500, 0, 1, kmRecorrido: 4);

        Assert.True(ServicioEnvioTienda.Ofrece(local));
        Assert.Equal(0, envio.Importe);
    }

    [Fact]
    public void Sin_precio_solo_permite_retiro()
    {
        var local = Local(precio: null);

        Assert.False(ServicioEnvioTienda.Ofrece(local));
        Assert.False(ServicioEnvioTienda.Cotizar(local, 1500, 0, 1, kmRecorrido: 2).Disponible);
    }

    [Fact]
    public void Sin_ubicacion_solo_permite_retiro()
    {
        var local = Local(ubicado: false);

        Assert.False(ServicioEnvioTienda.Ofrece(local));
        Assert.Contains("ubicación", ServicioEnvioTienda.Cotizar(local, 1500, 0, 1, 2).Motivo);
    }

    [Fact]
    public void Sin_recorrido_usa_la_linea_recta()
    {
        var envio = ServicioEnvioTienda.Cotizar(Local(precio: 10), 1500, latitud: 0, longitud: 1, kmRecorrido: null);

        Assert.Equal(ServicioEnvioTienda.Recta, envio.Origen);
        Assert.Equal(111.19m, envio.DistanciaKm);
        Assert.Equal(1111.90m, envio.Importe);
    }

    [Fact]
    public void Un_grado_en_el_ecuador_mide_111_19_km()
    {
        Assert.Equal(111.19m, ServicioEnvioTienda.DistanciaEnLineaRecta(0, 0, 0, 1));
    }

    [Fact]
    public void Asignar_sin_precio_ni_gratis_se_rechaza()
    {
        var error = Assert.Throws<ErrorNegocio>(() =>
            ServicioEnvioTienda.Asignar(Local(precio: null), false, null, null, null, null));

        Assert.Contains("precio por km", error.Message);
    }

    [Fact]
    public void Asignar_rechaza_un_radio_mayor_a_500_km()
    {
        Assert.Throws<ErrorNegocio>(() =>
            ServicioEnvioTienda.Asignar(Local(), false, 100, null, 501, null));
    }

    [Fact]
    public void El_envio_no_cambia_la_gestion()
    {
        var local = Local(precio: 100);
        local.GestionACargoComprador = true;
        var producto = new Producto
        {
            Id = 1, IdLocal = 1, Codigo = "7791", Nombre = "Yerba", Cantidad = 10, Precio = 1500, Costo = 1000, Publicado = true
        };

        var pedido = ServicioPedidosTienda.Crear(
            local, Ahora, 1, new DatosComprador("Ana Pérez", "11 4444-5555", "ana@correo.com"),
            [new ItemPedidoTienda(1, 1)], [producto], new Dictionary<int, decimal>(), false,
            ServicioPagoPedidoTienda.MercadoPago, false,
            new DatosEnvio(ServicioEnvioTienda.Envio, "Av. Rivadavia 1234", 0, 1, 2)).Pedido;

        Assert.Equal(1500, pedido.Subtotal);
        Assert.Equal(1000, pedido.Gestion);
        Assert.Equal(200, pedido.Envio);
        Assert.Equal(2700, pedido.Total);
        Assert.Equal(ServicioEnvioTienda.Envio, pedido.Modalidad);
        Assert.Equal("Av. Rivadavia 1234", pedido.DireccionEnvio);
    }

    [Fact]
    public void Entregar_suma_el_envio_a_la_venta()
    {
        var local = Local();
        var producto = new Producto
        {
            Id = 1, IdLocal = 1, Codigo = "7791", Nombre = "Yerba", Cantidad = 10, Precio = 1500, Costo = 1000, Publicado = true
        };
        var pedido = ServicioPedidosTienda.Crear(
            local, Ahora, 1, new DatosComprador("Ana Pérez", "11 4444-5555", "ana@correo.com"),
            [new ItemPedidoTienda(1, 1)], [producto], new Dictionary<int, decimal>(), false,
            envio: new DatosEnvio(ServicioEnvioTienda.Envio, "Av. Rivadavia 1234", 0, 1, 2)).Pedido;

        var venta = ServicioEntregaPedidoTienda.Entregar(pedido, [producto], Ahora);

        Assert.Equal(1700, venta.Total);
        Assert.Contains(venta.Lineas, l => l.Codigo == "ENVIO" && l.SubTotal == 200);
    }

    [Fact]
    public void Envio_fuera_de_radio_no_se_puede_pedir()
    {
        var local = Local(radio: 1);
        var producto = new Producto
        {
            Id = 1, IdLocal = 1, Codigo = "7791", Nombre = "Yerba", Cantidad = 10, Precio = 1500, Costo = 1000, Publicado = true
        };

        Assert.Throws<ErrorNegocio>(() => ServicioPedidosTienda.Crear(
            local, Ahora, 1, new DatosComprador("Ana Pérez", "11 4444-5555", "ana@correo.com"),
            [new ItemPedidoTienda(1, 1)], [producto], new Dictionary<int, decimal>(), false,
            envio: new DatosEnvio(ServicioEnvioTienda.Envio, "Av. Rivadavia 1234", 0, 1, 8)));
    }
}
