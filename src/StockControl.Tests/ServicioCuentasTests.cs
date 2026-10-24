using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCuentasTests
{
    private static Producto Comun(decimal stock = 10, decimal precio = 1500, decimal costo = 1000) =>
        new() { Id = 1, IdLocal = 1, Codigo = "779", Nombre = "Leche", Cantidad = stock, Precio = precio, Costo = costo };

    private static ResultadoCobro Cobrar(Producto producto, decimal cantidad, bool rigido = false, string? codigo = null) =>
        ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [producto],
            Items = [new ItemCarrito { IdProducto = producto.Id, Codigo = codigo ?? producto.Codigo, Nombre = producto.Nombre, Cantidad = cantidad, PrecioUnitario = 800 }],
            StockRigido = rigido,
            MetodoPago = "A cuenta"
        });

    private static CuentaPendiente Fiar(decimal importe = 3000, decimal? ganancia = 1000)
    {
        var resultado = new ResultadoCobro { Total = importe, Subtotal = importe };
        resultado.Lineas.Add(new LineaVenta { IdProducto = 1, Codigo = "779", Nombre = "Leche", Cantidad = 2, Precio = importe / 2, Costo = 1000, SubTotal = importe });
        var cuenta = ServicioCuentas.Fiar(1, 5, resultado, 1, 7, "Ana", ganancia);
        cuenta.Id = 9;
        return cuenta;
    }

    [Fact]
    public void Fiar_baja_el_stock_del_producto_comun()
    {
        var prod = Comun(stock: 10);

        ServicioCuentas.Fiar(1, 5, Cobrar(prod, 3), 1, 7, "Ana", null);

        Assert.Equal(7, prod.Cantidad);
    }

    [Fact]
    public void Fiar_sector_no_mueve_stock()
    {
        var prod = Comun(stock: 10);
        prod.ProductoSector = true;

        ServicioCuentas.Fiar(1, 5, Cobrar(prod, 1), 1, 7, "Ana", null);

        Assert.Equal(10, prod.Cantidad);
    }

    [Fact]
    public void Fiar_con_stock_rigido_sin_mercaderia_falla()
    {
        Assert.Throws<ErrorNegocio>(() => Cobrar(Comun(stock: 1), 3, rigido: true));
    }

    [Fact]
    public void La_cuenta_guarda_el_importe_del_dia_aunque_suba_el_precio()
    {
        var prod = Comun(precio: 1500);
        var cuenta = ServicioCuentas.Fiar(1, 5, Cobrar(prod, 2), 1, 7, "Ana", null);

        prod.Precio = 2000;

        Assert.Equal(3000, cuenta.Importe);
        Assert.Equal(3000, cuenta.Saldo);
        Assert.Equal(1500, cuenta.Lineas[0].Precio);
    }

    [Fact]
    public void Fiar_guarda_cliente_usuario_y_lineas()
    {
        var cuenta = Fiar();

        Assert.Equal(5, cuenta.IdCliente);
        Assert.Equal("Ana", cuenta.NombreUsuario);
        Assert.Equal(EstadoCuenta.Abierta, cuenta.Estado);
        Assert.Single(cuenta.Lineas);
    }

    [Fact]
    public void Pago_parcial_baja_el_saldo()
    {
        var cuenta = Fiar(3000);

        ServicioCuentas.Pagar(cuenta, "María", 1000, 7);

        Assert.Equal(2000, cuenta.Saldo);
        Assert.Equal(EstadoCuenta.Abierta, cuenta.Estado);
        Assert.Single(cuenta.Pagos);
    }

    [Fact]
    public void Pagar_el_saldo_cierra_la_cuenta()
    {
        var cuenta = Fiar(3000);
        ServicioCuentas.Pagar(cuenta, "María", 1000, 7);

        ServicioCuentas.Pagar(cuenta, "María", 2000, 7);

        Assert.Equal(0, cuenta.Saldo);
        Assert.Equal(EstadoCuenta.Pagada, cuenta.Estado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(3000.01)]
    public void Pagar_cero_o_mas_que_el_saldo_falla(decimal importe)
    {
        var cuenta = Fiar(3000);

        Assert.Throws<ErrorNegocio>(() => ServicioCuentas.Pagar(cuenta, "María", importe, 7));
        Assert.Equal(3000, cuenta.Saldo);
    }

    [Fact]
    public void Pagar_una_cuenta_cerrada_falla()
    {
        var cuenta = Fiar(3000);
        ServicioCuentas.Pagar(cuenta, "María", 3000, 7);

        Assert.Throws<ErrorNegocio>(() => ServicioCuentas.Pagar(cuenta, "María", 1, 7));
    }

    [Fact]
    public void Pagar_una_cuenta_incobrable_falla()
    {
        var cuenta = Fiar(3000);
        ServicioCuentas.Incobrable(cuenta, "dueño", DateTimeOffset.UtcNow);

        Assert.Throws<ErrorNegocio>(() => ServicioCuentas.Pagar(cuenta, "María", 1, 7));
    }

    [Fact]
    public void La_ganancia_del_pago_es_proporcional_a_lo_pagado()
    {
        var cuenta = Fiar(importe: 3000, ganancia: 1000);

        var (_, linea) = ServicioCuentas.Pagar(cuenta, "María", 1500, 7);

        Assert.Equal(1500, linea.SubTotal);
        Assert.Equal(1000, linea.Costo);
        Assert.Equal("CUENTA-9", linea.Codigo);
        Assert.Equal(1, linea.Cantidad);
    }

    [Fact]
    public void Pago_sin_ganancia_estimable_no_lleva_costo()
    {
        var cuenta = Fiar(importe: 3000, ganancia: null);

        var (_, linea) = ServicioCuentas.Pagar(cuenta, "María", 1500, 7);

        Assert.Null(linea.Costo);
    }

    [Fact]
    public void Devolver_un_pago_repone_el_saldo_y_reabre()
    {
        var cuenta = Fiar(3000);
        var (pago, _) = ServicioCuentas.Pagar(cuenta, "María", 3000, 7);

        ServicioCuentas.DevolverPago(cuenta, pago);

        Assert.Equal(3000, cuenta.Saldo);
        Assert.Equal(EstadoCuenta.Abierta, cuenta.Estado);
        Assert.Empty(cuenta.Pagos);
    }

    [Fact]
    public void Incobrable_deja_el_saldo_como_perdida()
    {
        var cuenta = Fiar(3000);
        ServicioCuentas.Pagar(cuenta, "María", 1000, 7);

        ServicioCuentas.Incobrable(cuenta, "dueño", DateTimeOffset.UtcNow);

        Assert.Equal(EstadoCuenta.Incobrable, cuenta.Estado);
        Assert.Equal(2000, cuenta.Saldo);
        Assert.Equal("dueño", cuenta.UsuarioIncobrable);
        Assert.Single(cuenta.Pagos);
    }

    [Fact]
    public void Incobrable_solo_desde_una_cuenta_abierta()
    {
        var cuenta = Fiar(3000);
        ServicioCuentas.Pagar(cuenta, "María", 3000, 7);

        Assert.Throws<ErrorNegocio>(() => ServicioCuentas.Incobrable(cuenta, "dueño", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Ganancia_estimada_usa_la_regla_de_estadisticas()
    {
        var prod = Comun(precio: 1500, costo: 1000);
        var resultado = Cobrar(prod, 2);

        var ganancia = ServicioCuentas.GananciaEstimada(resultado.Lineas, [prod], new FactoresGananciaLibre(1, 1));

        Assert.Equal(1000, ganancia);
    }

    [Fact]
    public void Deuda_por_cliente_suma_sus_cuentas_abiertas()
    {
        var a = Fiar(3000);
        var b = Fiar(1000);
        var otra = Fiar(500);
        otra.IdCliente = 6;
        var pagada = Fiar(800);
        ServicioCuentas.Pagar(pagada, "María", 800, 7);

        var deuda = ServicioCuentas.DeudaPorCliente([a, b, otra, pagada], new Dictionary<int, string> { [5] = "María", [6] = "José" });

        Assert.Equal(2, deuda.Count);
        Assert.Equal("María", deuda[0].Nombre);
        Assert.Equal(4000, deuda[0].Saldo);
        Assert.Equal(2, deuda[0].Cuentas);
    }

    [Fact]
    public void Cliente_sin_nombre_no_se_crea()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioCuentas.NuevoCliente(1, "  ", null));
    }

    [Theory]
    [InlineData(Roles.Dueno, false, true)]
    [InlineData(Roles.Socio, false, true)]
    [InlineData(Roles.Empleado, false, false)]
    [InlineData(Roles.Empleado, true, true)]
    public void Fiar_dueno_y_socio_siempre_empleado_si_esta_habilitado(string rol, bool habilitado, bool puede)
    {
        Assert.Equal(puede, Permisos.PuedeFiar(rol, habilitado));
    }

    [Fact]
    public void Solo_el_dueno_anula_una_cuenta()
    {
        Assert.True(Permisos.PuedeAnularCuenta(Roles.Dueno));
        Assert.False(Permisos.PuedeAnularCuenta(Roles.Empleado));
    }

    [Fact]
    public void Local_vencido_no_fia()
    {
        var local = new Local { IdLocal = 1, Nombre = "Centro", Vence = DateTimeOffset.UtcNow.AddDays(-10), EstadoAbono = ServicioAbono.AlDia };

        Assert.False(ServicioAbono.Estado(local, DateTimeOffset.UtcNow).PuedeOperarCaja);
    }
}
