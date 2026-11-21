using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioVentaSinConexionTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    private static Producto Comun(decimal stock = 10, decimal precio = 1500) =>
        new() { Id = 1, IdLocal = 1, Codigo = "779", Nombre = "Coca", Cantidad = stock, Precio = precio, Costo = 1000 };

    private static ResultadoCobro Cobrar(Producto prod, decimal cantidad, bool rigido, bool sinConexion, int descuento = 0) =>
        ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [prod],
            Items = [new ItemCarrito { IdProducto = prod.Id, Codigo = prod.Codigo, Nombre = prod.Nombre, Cantidad = cantidad, PrecioUnitario = 1 }],
            StockRigido = rigido,
            DesdeSinConexion = sinConexion,
            DescuentoPorcentaje = descuento
        });

    [Fact]
    public void Cobrar_sin_conexion_con_stock_rigido_insuficiente_entra_y_marca_conflicto()
    {
        var prod = Comun(stock: 1);
        var r = Cobrar(prod, 3, rigido: true, sinConexion: true);
        Assert.True(r.StockEnConflicto);
        Assert.Equal(4500, r.Total);
        Assert.Equal(0, prod.Cantidad);
    }

    [Fact]
    public void Cobrar_sin_conexion_con_stock_suficiente_no_marca_conflicto()
    {
        var r = Cobrar(Comun(stock: 5), 3, rigido: true, sinConexion: true);
        Assert.False(r.StockEnConflicto);
    }

    [Fact]
    public void Cobrar_con_stock_no_rigido_no_marca_conflicto()
    {
        var r = Cobrar(Comun(stock: 1), 3, rigido: false, sinConexion: true);
        Assert.False(r.StockEnConflicto);
    }

    [Fact]
    public void Cobrar_con_conexion_y_stock_rigido_insuficiente_rechaza()
    {
        Assert.Throws<ErrorNegocio>(() => Cobrar(Comun(stock: 1), 3, rigido: true, sinConexion: false));
    }

    [Fact]
    public void Cobrar_con_el_mismo_id_de_venta_no_duplica_la_venta()
    {
        var id = Guid.NewGuid();
        var guardada = new InformeVenta { IdInformeVenta = 7, IdLocal = 1, IdVentaCliente = id };
        var existente = ServicioVentaSinConexion.Existente([guardada], 1, id);
        Assert.Same(guardada, existente);
    }

    [Fact]
    public void Cobrar_con_el_mismo_id_de_venta_no_descuenta_stock_dos_veces()
    {
        var id = Guid.NewGuid();
        var prod = Comun(stock: 10);
        var ventas = new List<InformeVenta>();
        for (var envio = 0; envio < 2; envio++)
        {
            if (ServicioVentaSinConexion.Existente(ventas, 1, id) != null) continue;
            Cobrar(prod, 2, rigido: false, sinConexion: true);
            ventas.Add(new InformeVenta { IdLocal = 1, IdVentaCliente = id });
        }
        Assert.Equal(8, prod.Cantidad);
        Assert.Single(ventas);
    }

    [Fact]
    public void Cobrar_con_el_mismo_id_en_otro_local_crea_otra_venta()
    {
        var id = Guid.NewGuid();
        var deOtroLocal = new InformeVenta { IdLocal = 2, IdVentaCliente = id };
        Assert.Null(ServicioVentaSinConexion.Existente([deOtroLocal], 1, id));
    }

    [Fact]
    public void Cobrar_sin_id_de_venta_nunca_es_duplicada()
    {
        var sinId = new InformeVenta { IdLocal = 1 };
        Assert.Null(ServicioVentaSinConexion.Existente([sinId], 1, null));
        Assert.Null(ServicioVentaSinConexion.Existente([sinId], 1, Guid.Empty));
    }

    [Fact]
    public void Cobrar_sin_conexion_recalcula_el_total_con_el_precio_del_servidor()
    {
        var prod = Comun(precio: 1800);
        var r = Cobrar(prod, 2, rigido: false, sinConexion: true);
        Assert.Equal(3600, r.Total);
        var nota = ServicioVentaSinConexion.NotaRevision(false, 3000, r.Total);
        Assert.Contains("3000", nota);
        Assert.Contains("3600", nota);
    }

    [Fact]
    public void Cobrar_sin_conexion_con_el_mismo_total_no_deja_nota()
    {
        Assert.Null(ServicioVentaSinConexion.NotaRevision(false, 3000, 3000));
    }

    [Fact]
    public void Cobrar_sin_conexion_con_fecha_futura_usa_la_del_servidor()
    {
        Assert.Equal(Ahora, ServicioVentaSinConexion.FechaVenta(Ahora.AddHours(2), Ahora));
    }

    [Fact]
    public void Cobrar_sin_conexion_con_fecha_de_mas_de_7_dias_usa_la_del_servidor()
    {
        Assert.Equal(Ahora, ServicioVentaSinConexion.FechaVenta(Ahora.AddDays(-8), Ahora));
    }

    [Fact]
    public void Cobrar_sin_conexion_con_fecha_valida_usa_la_del_cobro()
    {
        var cobro = Ahora.AddHours(-3);
        Assert.Equal(cobro, ServicioVentaSinConexion.FechaVenta(cobro, Ahora));
    }

    [Fact]
    public void Cobrar_sin_conexion_con_descuento_sobre_margen_aplica_igual_que_online()
    {
        var offline = Cobrar(Comun(), 1, rigido: false, sinConexion: true, descuento: 50);
        var online = Cobrar(Comun(), 1, rigido: false, sinConexion: false, descuento: 50);
        Assert.Equal(online.Total, offline.Total);
    }

    [Fact]
    public void Cobrar_sin_conexion_producto_sector_y_generico_entran_sin_conflicto()
    {
        var sector = Comun(stock: 0);
        sector.ProductoSector = true;
        sector.Codigo = "SEC-1";
        var r = Cobrar(sector, 1, rigido: true, sinConexion: true);
        Assert.False(r.StockEnConflicto);

        var generico = ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [],
            Items = [new ItemCarrito { Codigo = "GENERIC-1", Nombre = "Varios", Cantidad = 1, PrecioUnitario = 500 }],
            StockRigido = true,
            DesdeSinConexion = true
        });
        Assert.False(generico.StockEnConflicto);
        Assert.Equal(500, generico.Total);
    }

    [Fact]
    public void Revisar_venta_solo_dueno_y_socio_no_empleado()
    {
        Assert.True(Permisos.PuedeRevisarVentas(Roles.Dueno));
        Assert.True(Permisos.PuedeRevisarVentas(Roles.Socio));
        Assert.False(Permisos.PuedeRevisarVentas(Roles.Empleado));
    }

    [Fact]
    public void Venta_revisada_sale_del_listado_para_revisar()
    {
        var venta = new InformeVenta { StockEnConflicto = true, NotaRevision = "Stock insuficiente al sincronizar." };
        Assert.True(venta.ParaRevisar);
        venta.MarcarRevisada();
        Assert.False(venta.ParaRevisar);
    }
}

