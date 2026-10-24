using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioOfertasTests
{
    private static readonly DateOnly Hoy = new(2026, 10, 23);

    private static Producto Comun(decimal stock = 10, decimal precio = 1500, decimal costo = 1000) =>
        new() { Id = 1, IdLocal = 1, Codigo = "779", Nombre = "Coca", Cantidad = stock, Precio = precio, Costo = costo };

    private static Oferta Descuento(int porcentaje, DateOnly? desde = null, DateOnly? hasta = null) =>
        new() { Id = 1, IdProducto = 1, Tipo = TipoOferta.Descuento, Porcentaje = porcentaje, FechaDesde = desde ?? Hoy, FechaHasta = hasta ?? Hoy };

    private static Oferta LlevaPaga(int lleva, int paga, DateOnly? desde = null, DateOnly? hasta = null) =>
        new() { Id = 1, IdProducto = 1, Tipo = TipoOferta.LlevaPaga, Lleva = lleva, Paga = paga, FechaDesde = desde ?? Hoy, FechaHasta = hasta ?? Hoy };

    private static ResultadoCobro Cobrar(Producto producto, decimal cantidad, Oferta? oferta, int descuento = 0, bool alCosto = false, bool sinOfertas = false) =>
        ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [producto],
            Items = [new ItemCarrito { IdProducto = 1, Codigo = "779", Nombre = "Coca", Cantidad = cantidad, PrecioUnitario = producto.Precio }],
            DescuentoPorcentaje = descuento,
            CobrarAlCosto = alCosto,
            Ofertas = oferta == null ? [] : [oferta],
            Hoy = Hoy,
            SinOfertas = sinOfertas
        });

    [Fact]
    public void Descuento_aplica_el_porcentaje_al_precio_de_lista()
    {
        var r = Cobrar(Comun(), 2, Descuento(20));

        Assert.Equal(1200, r.Lineas[0].Precio);
        Assert.Equal(2400, r.Total);
        Assert.Equal("20 % off", r.Lineas[0].DescripcionOferta);
    }

    [Fact]
    public void Descuento_descuenta_el_stock_normal()
    {
        var prod = Comun(stock: 10);

        Cobrar(prod, 2, Descuento(20));

        Assert.Equal(8, prod.Cantidad);
    }

    [Theory]
    [InlineData(3, 2)]
    [InlineData(4, 3)]
    [InlineData(6, 4)]
    [InlineData(2, 2)]
    public void Tres_por_dos_cobra_cada_grupo_de_tres_como_dos(int llevadas, int cobradas)
    {
        var r = Cobrar(Comun(precio: 1000, costo: 600), llevadas, LlevaPaga(3, 2));

        Assert.Equal(cobradas * 1000, r.Total);
        Assert.Equal(llevadas, r.Lineas[0].Cantidad);
        Assert.Equal("3x2", r.Lineas[0].DescripcionOferta);
    }

    [Fact]
    public void Promo_descuenta_del_stock_lo_que_se_lleva()
    {
        var prod = Comun(stock: 10);

        Cobrar(prod, 3, LlevaPaga(3, 2));

        Assert.Equal(7, prod.Cantidad);
    }

    [Fact]
    public void Oferta_fuera_de_fecha_no_corre()
    {
        var r = Cobrar(Comun(), 1, Descuento(20, Hoy.AddDays(1), Hoy.AddDays(5)));

        Assert.Equal(1500, r.Total);
        Assert.Equal("", r.Lineas[0].DescripcionOferta);
    }

    [Fact]
    public void Oferta_corre_el_primer_y_el_ultimo_dia()
    {
        var oferta = Descuento(20, Hoy.AddDays(-3), Hoy.AddDays(3));

        Assert.NotNull(ServicioOfertas.Vigente([oferta], 1, Hoy.AddDays(-3)));
        Assert.NotNull(ServicioOfertas.Vigente([oferta], 1, Hoy.AddDays(3)));
        Assert.Null(ServicioOfertas.Vigente([oferta], 1, Hoy.AddDays(4)));
    }

    [Fact]
    public void Primero_corre_la_oferta_y_despues_el_descuento_manual_sobre_margen()
    {
        var r = Cobrar(Comun(precio: 1500, costo: 1000), 1, Descuento(10), descuento: 50);

        Assert.Equal(1175, r.Total);
    }

    [Fact]
    public void Al_costo_gana_a_la_oferta()
    {
        var r = Cobrar(Comun(precio: 1500, costo: 1000), 3, LlevaPaga(3, 2), alCosto: true);

        Assert.Equal(3000, r.Total);
        Assert.Equal("", r.Lineas[0].DescripcionOferta);
    }

    [Fact]
    public void Descuento_de_caja_deshabilita_la_oferta_si_esta_marcada()
    {
        var oferta = Descuento(10);
        oferta.AnulaConDescuentoCaja = true;

        var r = Cobrar(Comun(precio: 1500, costo: 1000), 1, oferta, descuento: 50);

        Assert.Equal(1250, r.Total);
        Assert.Equal("", r.Lineas[0].DescripcionOferta);
    }

    [Fact]
    public void Al_costo_sin_deshabilitar_mantiene_la_promo()
    {
        var oferta = LlevaPaga(3, 2);
        oferta.AnulaConAlCosto = false;

        var r = Cobrar(Comun(precio: 1500, costo: 1000), 3, oferta, alCosto: true);

        Assert.Equal(2000, r.Total);
        Assert.Equal("3x2", r.Lineas[0].DescripcionOferta);
    }

    [Fact]
    public void Al_costo_sin_deshabilitar_aplica_el_descuento_sobre_el_costo()
    {
        var oferta = Descuento(10);
        oferta.AnulaConAlCosto = false;

        var r = Cobrar(Comun(precio: 1500, costo: 1000), 1, oferta, alCosto: true);

        Assert.Equal(900, r.Total);
    }

    [Fact]
    public void No_aplicar_ofertas_en_caja_saca_la_oferta_que_lo_permite()
    {
        var oferta = LlevaPaga(3, 2);
        oferta.PermiteNoAplicarEnCaja = true;

        var r = Cobrar(Comun(precio: 1000, costo: 600), 3, oferta, sinOfertas: true);

        Assert.Equal(3000, r.Total);
    }

    [Fact]
    public void No_aplicar_ofertas_no_saca_la_oferta_que_no_lo_permite()
    {
        var r = Cobrar(Comun(precio: 1000, costo: 600), 3, LlevaPaga(3, 2), sinOfertas: true);

        Assert.Equal(2000, r.Total);
    }

    [Fact]
    public void Sector_no_toma_la_oferta_al_cobrar()
    {
        var prod = Comun();
        prod.ProductoSector = true;

        var r = Cobrar(prod, 1, Descuento(20));

        Assert.Equal(1500, r.Total);
    }

    [Fact]
    public void Validar_rechaza_oferta_de_producto_sector()
    {
        var prod = Comun();
        prod.ProductoSector = true;

        Assert.Throws<ErrorNegocio>(() => ServicioOfertas.Validar(Descuento(20), prod, []));
    }

    [Fact]
    public void Validar_rechaza_oferta_de_producto_generico()
    {
        var prod = Comun();
        prod.Codigo = "GENERIC-ab12";

        Assert.Throws<ErrorNegocio>(() => ServicioOfertas.Validar(Descuento(20), prod, []));
    }

    [Fact]
    public void Validar_rechaza_producto_inexistente()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioOfertas.Validar(Descuento(20), null, []));
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 0)]
    public void Validar_rechaza_llevas_que_no_supera_a_pagas(int lleva, int paga)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioOfertas.Validar(LlevaPaga(lleva, paga), Comun(), []));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Validar_rechaza_porcentaje_fuera_de_rango(int porcentaje)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioOfertas.Validar(Descuento(porcentaje), Comun(), []));
    }

    [Fact]
    public void Validar_rechaza_fechas_invertidas()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioOfertas.Validar(Descuento(20, Hoy, Hoy.AddDays(-1)), Comun(), []));
    }

    [Fact]
    public void Validar_rechaza_ofertas_que_se_pisan_en_fechas()
    {
        var existente = Descuento(10, Hoy, Hoy.AddDays(10));
        existente.Id = 2;
        var nueva = LlevaPaga(3, 2, Hoy.AddDays(10), Hoy.AddDays(20));
        nueva.Id = 0;

        var ex = Assert.Throws<ErrorNegocio>(() => ServicioOfertas.Validar(nueva, Comun(), [existente]));

        Assert.Contains("esas fechas", ex.Message);
    }

    [Fact]
    public void Validar_acepta_ofertas_que_no_se_pisan()
    {
        var existente = Descuento(10, Hoy, Hoy.AddDays(10));
        existente.Id = 2;
        var nueva = LlevaPaga(3, 2, Hoy.AddDays(11), Hoy.AddDays(20));
        nueva.Id = 0;

        ServicioOfertas.Validar(nueva, Comun(), [existente]);
    }

    [Fact]
    public void Editar_una_oferta_no_choca_consigo_misma()
    {
        var oferta = Descuento(10, Hoy, Hoy.AddDays(10));

        ServicioOfertas.Validar(oferta, Comun(), [oferta]);
    }

    private static Oferta PrecioPor(string tipo, int lleva, decimal precio, DateOnly? hasta = null) =>
        new() { Id = 1, IdProducto = 1, Tipo = tipo, Lleva = lleva, PrecioUnidad = precio, FechaDesde = Hoy, FechaHasta = hasta ?? Hoy };

    [Theory]
    [InlineData(3, 3000)]
    [InlineData(4, 40)]
    [InlineData(7, 70)]
    public void Precio_desde_aplica_de_N_unidades_en_adelante(int cantidad, decimal total)
    {
        var r = Cobrar(Comun(precio: 1000), cantidad, PrecioPor(TipoOferta.PrecioDesde, 4, 10));

        Assert.Equal(total, r.Total);
    }

    [Theory]
    [InlineData(3, 3000)]
    [InlineData(4, 40)]
    [InlineData(5, 5000)]
    public void Precio_exacto_aplica_solo_con_N_unidades(int cantidad, decimal total)
    {
        var r = Cobrar(Comun(precio: 1000), cantidad, PrecioPor(TipoOferta.PrecioExacto, 4, 10));

        Assert.Equal(total, r.Total);
    }

    [Fact]
    public void Precio_por_cantidad_muestra_la_descripcion_y_no_cambia_el_stock()
    {
        var prod = Comun(stock: 10, precio: 1000);

        var r = Cobrar(prod, 4, PrecioPor(TipoOferta.PrecioDesde, 4, 10));

        Assert.Equal("4+ a $10", r.Lineas[0].DescripcionOferta);
        Assert.Equal(6, prod.Cantidad);
    }

    [Fact]
    public void Precio_por_cantidad_se_anula_con_descuento_de_caja_si_la_oferta_lo_pide()
    {
        var oferta = PrecioPor(TipoOferta.PrecioDesde, 4, 10);
        oferta.AnulaConDescuentoCaja = true;

        var r = Cobrar(Comun(precio: 1000, costo: 600), 4, oferta, descuento: 10);

        Assert.Equal(3840, r.Total);
    }

    [Theory]
    [InlineData(TipoOferta.PrecioDesde, 1, 10)]
    [InlineData(TipoOferta.PrecioExacto, 4, 0)]
    public void Validar_rechaza_precio_por_cantidad_incorrecto(string tipo, int lleva, decimal precio)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioOfertas.Validar(PrecioPor(tipo, lleva, precio), Comun(), []));
    }

    [Fact]
    public void Oferta_sin_fin_sigue_vigente_en_cualquier_fecha_posterior()
    {
        var oferta = Descuento(10, Hoy, DateOnly.MaxValue);

        Assert.True(oferta.SinFin);
        Assert.NotNull(ServicioOfertas.Vigente([oferta], 1, Hoy.AddYears(5)));
    }

    [Fact]
    public void Oferta_sin_fin_pisa_a_otra_que_empieza_despues()
    {
        var sinFin = Descuento(10, Hoy, DateOnly.MaxValue);
        sinFin.Id = 2;
        var nueva = Descuento(15, Hoy.AddDays(30), Hoy.AddDays(40));
        nueva.Id = 0;

        Assert.Throws<ErrorNegocio>(() => ServicioOfertas.Validar(nueva, Comun(), [sinFin]));
    }

    [Fact]
    public void Solo_el_dueno_administra_ofertas()
    {
        Assert.True(Permisos.PuedeAdministrarOfertas(Roles.Dueno));
        Assert.False(Permisos.PuedeAdministrarOfertas(Roles.Empleado));
    }
}
