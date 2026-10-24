using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioPacksProveedorTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);

    private static Producto Coca(int idProveedor = 0, decimal stock = 5) =>
        new() { Id = 1, IdLocal = 1, Codigo = "779", Nombre = "Coca 1.75 lt", IdProveedor = idProveedor, Cantidad = stock };

    private static OrdenCompra Armar(params LineaOrdenPedida[] lineas) =>
        ServicioOrdenCompra.Armar(7, 1, lineas, [Coca()], 1, "dueno", Ahora);

    [Fact]
    public void Linea_por_unidad_queda_sin_pack()
    {
        var linea = Assert.Single(Armar(new LineaOrdenPedida { IdProducto = 1, Cantidad = 3 }).Lineas);

        Assert.Equal("", linea.NombrePack);
        Assert.Equal(1, linea.UnidadesPack);
    }

    [Fact]
    public void Linea_por_pack_guarda_nombre_y_unidades()
    {
        var linea = Assert.Single(Armar(new LineaOrdenPedida { IdProducto = 1, Cantidad = 2, NombrePack = " Pack ", UnidadesPack = 6 }).Lineas);

        Assert.Equal("Pack", linea.NombrePack);
        Assert.Equal(6, linea.UnidadesPack);
        Assert.Equal(2m, linea.Cantidad);
    }

    [Theory]
    [InlineData("", 6)]
    [InlineData("Pack", 1)]
    [InlineData("Pack", 0)]
    [InlineData("Pack", 10001)]
    public void Pack_incompleto_o_fuera_de_rango_da_error(string nombre, int unidades)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioVinculosProveedor.NormalizarPack(nombre, unidades));
    }

    [Fact]
    public void Nombre_de_pack_demasiado_largo_da_error()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioVinculosProveedor.NormalizarPack(new string('a', 31), 6));
    }

    [Fact]
    public void Whatsapp_dice_cantidad_pack_de_producto_sin_codigo()
    {
        var orden = Armar(new LineaOrdenPedida { IdProducto = 1, Cantidad = 2, NombrePack = "Pack", UnidadesPack = 6 });

        var texto = ServicioOrdenCompra.TextoWhatsApp("Kiosco", "Distri", Ahora, orden.Lineas);

        Assert.Contains("- 2 Pack de Coca 1.75 lt", texto);
        Assert.DoesNotContain("779", texto);
    }

    [Fact]
    public void Whatsapp_por_unidad_dice_cantidad_x_producto_sin_codigo()
    {
        var orden = Armar(new LineaOrdenPedida { IdProducto = 1, Cantidad = 3 });

        var texto = ServicioOrdenCompra.TextoWhatsApp("Kiosco", "Distri", Ahora, orden.Lineas);

        Assert.Contains("- 3 x Coca 1.75 lt", texto);
        Assert.DoesNotContain("779", texto);
    }

    [Fact]
    public void Recibir_packs_suma_al_stock_packs_por_unidades()
    {
        var producto = Coca(stock: 5);
        var orden = Armar(new LineaOrdenPedida { IdProducto = 1, Cantidad = 2, NombrePack = "Pack", UnidadesPack = 6 });
        orden.Lineas[0].Id = 10;

        ServicioRecepcionOrden.Recibir(orden, [new LineaRecibida { IdLinea = 10, Cantidad = 2 }], [producto], 1, "dueno", Ahora);

        Assert.Equal(17m, producto.Cantidad);
        Assert.Equal(2m, orden.Lineas[0].CantidadRecibida);
    }

    [Fact]
    public void Recibir_menos_packs_de_los_pedidos_suma_solo_lo_recibido()
    {
        var producto = Coca(stock: 0);
        var orden = Armar(new LineaOrdenPedida { IdProducto = 1, Cantidad = 3, NombrePack = "Caja", UnidadesPack = 12 });
        orden.Lineas[0].Id = 10;

        ServicioRecepcionOrden.Recibir(orden, [new LineaRecibida { IdLinea = 10, Cantidad = 1 }], [producto], 1, "dueno", Ahora);

        Assert.Equal(12m, producto.Cantidad);
    }

    [Fact]
    public void Producto_no_vinculado_se_vincula_con_su_pack()
    {
        var nuevo = ServicioVinculosProveedor.Asegurar([], Coca(), 7, "Pack", 6);

        Assert.NotNull(nuevo);
        Assert.Equal(7, nuevo.IdProveedor);
        Assert.Equal(1, nuevo.IdProducto);
        Assert.Equal(6, nuevo.UnidadesPack);
    }

    [Fact]
    public void Producto_no_vinculado_por_unidad_tambien_se_vincula()
    {
        var nuevo = ServicioVinculosProveedor.Asegurar([], Coca(), 7, "", 1);

        Assert.NotNull(nuevo);
        Assert.Equal("", nuevo.NombrePack);
        Assert.Equal(1, nuevo.UnidadesPack);
    }

    [Fact]
    public void Producto_ya_vinculado_no_crea_otro_vinculo()
    {
        var existente = new ProductoProveedor { IdProducto = 1, IdProveedor = 7, NombrePack = "Pack", UnidadesPack = 6 };

        Assert.Null(ServicioVinculosProveedor.Asegurar([existente], Coca(), 7, "Caja", 12));
    }

    [Fact]
    public void Producto_del_catalogo_del_proveedor_por_unidad_no_necesita_vinculo()
    {
        Assert.Null(ServicioVinculosProveedor.Asegurar([], Coca(idProveedor: 7), 7, "", 1));
    }

    [Fact]
    public void Producto_del_catalogo_del_proveedor_pedido_por_pack_guarda_el_pack()
    {
        var nuevo = ServicioVinculosProveedor.Asegurar([], Coca(idProveedor: 7), 7, "Pack", 6);

        Assert.NotNull(nuevo);
    }

    [Fact]
    public void Vincular_en_otro_proveedor_no_toca_el_proveedor_del_catalogo()
    {
        var producto = Coca(idProveedor: 3);

        var nuevo = ServicioVinculosProveedor.Asegurar([], producto, 7, "Pack", 6);

        Assert.NotNull(nuevo);
        Assert.Equal(3, producto.IdProveedor);
    }

    [Fact]
    public void Fijar_modifica_nombre_y_unidades_del_pack_existente()
    {
        var existente = new ProductoProveedor { IdProducto = 1, IdProveedor = 7, NombrePack = "Pack", UnidadesPack = 6 };

        var resultado = ServicioVinculosProveedor.Fijar(existente, Coca(), 7, "Caja", 12);

        Assert.Same(existente, resultado);
        Assert.Equal("Caja", resultado.NombrePack);
        Assert.Equal(12, resultado.UnidadesPack);
    }

    [Fact]
    public void Fijar_sin_vinculo_previo_lo_crea_y_por_unidad_borra_el_pack()
    {
        var existente = new ProductoProveedor { IdProducto = 1, IdProveedor = 7, NombrePack = "Pack", UnidadesPack = 6 };

        var nuevo = ServicioVinculosProveedor.Fijar(null, Coca(), 7, "Caja", 12);
        var unidad = ServicioVinculosProveedor.Fijar(existente, Coca(), 7, "", 1);

        Assert.Equal(7, nuevo.IdProveedor);
        Assert.Equal("Caja", nuevo.NombrePack);
        Assert.Equal("", unidad.NombrePack);
        Assert.Equal(1, unidad.UnidadesPack);
    }

    [Fact]
    public void Solo_dueno_y_socio_administran_proveedores()
    {
        Assert.False(Permisos.PuedeAdministrarProveedores(Roles.Empleado));
    }
}
