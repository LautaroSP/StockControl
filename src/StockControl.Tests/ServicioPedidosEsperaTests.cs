using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioPedidosEsperaTests
{
    private static Producto Comun(int id = 1, decimal stock = 10, decimal precio = 1500, decimal costo = 1000) =>
        new() { Id = id, IdLocal = 1, Codigo = $"77{id}", Nombre = $"Producto {id}", Cantidad = stock, Precio = precio, Costo = costo };

    private static ItemCarrito Item(int idProducto, string codigo, decimal precio, decimal cantidad = 1) =>
        new() { IdProducto = idProducto, Codigo = codigo, Nombre = codigo, Cantidad = cantidad, PrecioUnitario = precio };

    private static PedidoEspera Guardar(params ItemCarrito[] items) =>
        ServicioPedidosEspera.Guardar(1, 2, 7, "María", items, 0, false);

    [Fact]
    public void Guardar_rechaza_carrito_vacio()
    {
        var ex = Assert.Throws<ErrorNegocio>(() => Guardar());

        Assert.Contains("vacío", ex.Message);
    }

    [Fact]
    public void Guardar_conserva_puesto_nota_descuento_y_al_costo()
    {
        var pedido = ServicioPedidosEspera.Guardar(1, 3, 7, "  pedido de las 11 ", [Item(1, "771", 1500)], 20, true);

        Assert.Equal(3, pedido.NroCaja);
        Assert.Equal("pedido de las 11", pedido.Nota);
        Assert.Equal(20, pedido.DescuentoPorcentaje);
        Assert.True(pedido.AlCosto);
    }

    [Fact]
    public void Guardar_conserva_precio_de_generico_y_sector()
    {
        var pedido = Guardar(Item(0, "GENERIC-ab12", 350, 2), Item(5, "SEC-1", 820));

        Assert.Equal(350, pedido.Lineas[0].PrecioUnitario);
        Assert.Equal(2, pedido.Lineas[0].Cantidad);
        Assert.Equal(820, pedido.Lineas[1].PrecioUnitario);
    }

    [Fact]
    public void Guardar_rechaza_nota_de_mas_de_80_caracteres()
    {
        Assert.Throws<ErrorNegocio>(() =>
            ServicioPedidosEspera.Guardar(1, 1, null, new string('x', 81), [Item(1, "771", 1500)], 0, false));
    }

    [Fact]
    public void Guardar_rechaza_cantidad_cero()
    {
        Assert.Throws<ErrorNegocio>(() => Guardar(Item(1, "771", 1500, 0)));
    }

    [Fact]
    public void Un_puesto_no_guarda_mas_del_maximo()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioPedidosEspera.ExigirLugar(ServicioPedidosEspera.MaximoPorPuesto));
    }

    [Fact]
    public void Cargar_toma_el_precio_actual_y_avisa_si_cambio()
    {
        var pedido = Guardar(Item(1, "771", 1500));

        var cargado = ServicioPedidosEspera.Cargar(pedido, [Comun(precio: 1800)]);

        Assert.Equal(1800, cargado.Lineas[0].PrecioUnitario);
        Assert.Equal(1500, cargado.Lineas[0].PrecioAnterior);
    }

    [Fact]
    public void Cargar_sin_cambio_de_precio_no_avisa()
    {
        var pedido = Guardar(Item(1, "771", 1500));

        var cargado = ServicioPedidosEspera.Cargar(pedido, [Comun(precio: 1500)]);

        Assert.Null(cargado.Lineas[0].PrecioAnterior);
    }

    [Fact]
    public void Cargar_sector_y_generico_conservan_el_precio_guardado()
    {
        var sector = Comun(id: 5, precio: 0);
        sector.ProductoSector = true;
        var pedido = Guardar(Item(5, "SEC-1", 820), Item(0, "GENERIC-ab12", 350));

        var cargado = ServicioPedidosEspera.Cargar(pedido, [sector]);

        Assert.Equal(820, cargado.Lineas[0].PrecioUnitario);
        Assert.True(cargado.Lineas[0].Sector);
        Assert.Equal(350, cargado.Lineas[1].PrecioUnitario);
        Assert.True(cargado.Lineas[1].Generico);
        Assert.All(cargado.Lineas, l => Assert.Null(l.PrecioAnterior));
    }

    [Fact]
    public void Cargar_no_mueve_stock_y_el_pedido_sigue_guardado()
    {
        var prod = Comun(stock: 10);
        var pedido = Guardar(Item(1, "771", 1500, 3));

        ServicioPedidosEspera.Cargar(pedido, [prod]);

        Assert.Equal(10, prod.Cantidad);
        Assert.Single(pedido.Lineas);
    }

    [Fact]
    public void Cargar_informa_producto_borrado_y_no_lo_carga()
    {
        var pedido = Guardar(Item(1, "771", 1500), Item(9, "Borrado", 200));

        var cargado = ServicioPedidosEspera.Cargar(pedido, [Comun()]);

        Assert.Single(cargado.Lineas);
        Assert.Equal(["Borrado"], cargado.Faltantes);
    }

    [Fact]
    public void Otro_puesto_no_ve_el_pedido()
    {
        var guardados = new[]
        {
            ServicioPedidosEspera.Guardar(1, 1, null, "Puesto 1", [Item(1, "771", 1500)], 0, false),
            ServicioPedidosEspera.Guardar(1, 2, null, "Puesto 2", [Item(1, "771", 1500)], 0, false)
        }.AsQueryable();

        var delPuesto = guardados.DelPuesto(2).ToList();

        Assert.Equal("Puesto 2", Assert.Single(delPuesto).Nota);
    }

    [Fact]
    public void Cobrar_un_pedido_ya_borrado_avisa_que_ya_se_cobro()
    {
        ServicioPedidosEspera.ExigirBorrado(1);

        var ex = Assert.Throws<PedidoYaCobrado>(() => ServicioPedidosEspera.ExigirBorrado(0));

        Assert.Contains("ya se cobró", ex.Message);
    }

    [Fact]
    public void Pedido_cargado_con_stock_rigido_sin_mercaderia_no_se_cobra()
    {
        var prod = Comun(stock: 1);
        var cargado = ServicioPedidosEspera.Cargar(Guardar(Item(1, "771", 1500, 3)), [prod]);

        Assert.Throws<ErrorNegocio>(() => ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [prod],
            Items = cargado.Lineas.Select(l => new ItemCarrito
            {
                IdProducto = l.IdProducto,
                Codigo = l.Codigo,
                Nombre = l.Nombre,
                Cantidad = l.Cantidad,
                PrecioUnitario = l.PrecioUnitario
            }).ToList(),
            StockRigido = true
        }));
        Assert.Equal(1, prod.Cantidad);
    }
}
