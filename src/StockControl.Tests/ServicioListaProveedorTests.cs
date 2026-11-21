using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioListaProveedorTests
{
    const int Proveedor = 5;
    const decimal FactorLocal = 1.5m;
    const decimal Iva = 1.21m;

    static Producto Producto(int id, string codigo, decimal costo = 100, decimal precio = 200, int proveedor = Proveedor) => new()
    {
        Id = id,
        IdLocal = 1,
        Codigo = codigo,
        Nombre = "Producto " + id,
        Costo = costo,
        Precio = precio,
        IdProveedor = proveedor,
        Cantidad = 10
    };

    static FilaLista Fila(int nro, string codigo, decimal? costo) => new() { Fila = nro, Codigo = codigo, Costo = costo };

    static ProductoProveedor Pack(int idProducto, int unidades) =>
        new() { IdProducto = idProducto, IdProveedor = Proveedor, UnidadesPack = unidades };

    static RevisionLista Revisar(FilaLista[] filas, Producto[] productos, GrupoProductos[]? grupos = null, ProductoProveedor[]? packs = null) =>
        ServicioListaProveedor.Revisar(filas, productos, grupos ?? [], packs ?? [], Proveedor, FactorLocal, Iva);

    static RevisionLista Aplicar(FilaLista[] filas, Producto[] productos, GrupoProductos[]? grupos = null, ProductoProveedor[]? packs = null) =>
        ServicioListaProveedor.Aplicar(filas, productos, grupos ?? [], packs ?? [], Proveedor, FactorLocal, Iva, "ana");

    [Fact]
    public void Costo_pack_se_divide_por_unidades_del_pack()
    {
        var p = Producto(1, "A1");
        Aplicar([Fila(2, "A1", 120)], [p], packs: [Pack(1, 12)]);
        Assert.Equal(10m, p.Costo);
    }

    [Fact]
    public void Costo_sin_pack_es_por_unidad()
    {
        var p = Producto(1, "A1");
        Aplicar([Fila(2, "A1", 120)], [p]);
        Assert.Equal(120m, p.Costo);
    }

    [Fact]
    public void Precio_usa_factor_del_local_por_iva()
    {
        var p = Producto(1, "A1");
        Aplicar([Fila(2, "A1", 100)], [p]);
        Assert.Equal(181.5m, p.Precio);
        Assert.Equal("ana", p.UsuarioModificacion);
    }

    [Fact]
    public void Precio_usa_ganancia_propia_si_es_individual()
    {
        var p = Producto(1, "A1");
        p.GananciaIndividual = true;
        p.ValorGanancia = 2m;
        Aplicar([Fila(2, "A1", 100)], [p]);
        Assert.Equal(242m, p.Precio);
    }

    [Fact]
    public void Producto_sector_se_saltea()
    {
        var p = Producto(1, "A1");
        p.ProductoSector = true;
        var r = Aplicar([Fila(2, "A1", 500)], [p]);
        Assert.Single(r.Salteados);
        Assert.Equal(100m, p.Costo);
        Assert.Equal(200m, p.Precio);
    }

    [Fact]
    public void Codigo_no_encontrado_no_crea_producto()
    {
        var productos = new[] { Producto(1, "A1") };
        var r = Aplicar([Fila(2, "ZZ", 100)], productos);
        Assert.Single(r.NoEncontrados);
        Assert.Equal("ZZ", r.NoEncontrados[0].Codigo);
        Assert.Single(productos);
        Assert.Empty(r.Encontrados);
    }

    [Fact]
    public void Lista_no_suma_stock()
    {
        var p = Producto(1, "A1");
        Aplicar([Fila(2, "A1", 100)], [p]);
        Assert.Equal(10m, p.Cantidad);
    }

    [Fact]
    public void Codigo_de_otro_proveedor_se_saltea()
    {
        var p = Producto(1, "A1", proveedor: 9);
        var r = Aplicar([Fila(2, "A1", 500)], [p]);
        Assert.Single(r.Salteados);
        Assert.Equal(9, p.IdProveedor);
        Assert.Equal(100m, p.Costo);
    }

    [Fact]
    public void Codigo_repetido_en_excel_se_saltea()
    {
        var p = Producto(1, "A1");
        var r = Aplicar([Fila(2, "A1", 100), Fila(3, "A1", 900)], [p]);
        Assert.Single(r.Encontrados);
        Assert.Single(r.Salteados);
        Assert.Equal(100m, p.Costo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-5)]
    public void Costo_invalido_se_saltea(int? costo)
    {
        var p = Producto(1, "A1");
        var r = Aplicar([Fila(2, "A1", costo)], [p]);
        Assert.Single(r.Salteados);
        Assert.Equal(100m, p.Costo);
    }

    [Fact]
    public void Producto_generico_se_saltea()
    {
        var p = Producto(1, "GENERIC-1");
        var r = Aplicar([Fila(2, "GENERIC-1", 100)], [p]);
        Assert.Single(r.Salteados);
    }

    [Fact]
    public void Grupo_se_actualiza_una_vez_y_copia_a_miembros()
    {
        var grupo = new GrupoProductos { IdGrupoProducto = 7, NombreGrupo = "Gaseosas", Costo = 100, PrecioGrupo = 200 };
        var a = Producto(1, "A1");
        var b = Producto(2, "A2");
        var c = Producto(3, "A3");
        a.IdGrupoProducto = b.IdGrupoProducto = c.IdGrupoProducto = 7;
        var r = Aplicar([Fila(2, "A1", 100), Fila(3, "A2", 999)], [a, b, c], [grupo]);
        Assert.Single(r.Encontrados);
        Assert.Single(r.Salteados);
        Assert.Equal(100m, grupo.Costo);
        Assert.Equal(181.5m, grupo.PrecioGrupo);
        Assert.All(new[] { a, b, c }, p =>
        {
            Assert.Equal(100m, p.Costo);
            Assert.Equal(181.5m, p.Precio);
        });
    }

    [Fact]
    public void Grupo_usa_su_ganancia_si_es_individual()
    {
        var grupo = new GrupoProductos { IdGrupoProducto = 7, Costo = 50, PrecioGrupo = 80, GananciaIndividual = true, Ganancia = 2m };
        var a = Producto(1, "A1");
        a.IdGrupoProducto = 7;
        Aplicar([Fila(2, "A1", 100)], [a], [grupo]);
        Assert.Equal(242m, grupo.PrecioGrupo);
        Assert.Equal(242m, a.Precio);
    }

    [Fact]
    public void Vista_previa_coincide_con_lo_aplicado()
    {
        var p = Producto(1, "A1");
        var filas = new[] { Fila(2, "A1", 120), Fila(3, "ZZ", 5) };
        var packs = new[] { Pack(1, 12) };
        var previa = Revisar(filas, [p], packs: packs);
        Assert.Equal(100m, p.Costo);
        var aplicada = Aplicar(filas, [p], packs: packs);
        var e = previa.Encontrados[0];
        Assert.Equal(p.Costo, e.CostoNuevo);
        Assert.Equal(p.Precio, e.PrecioNuevo);
        Assert.Equal(previa.NoEncontrados.Count, aplicada.NoEncontrados.Count);
    }

    [Fact]
    public void Revisar_no_toca_los_productos()
    {
        var p = Producto(1, "A1");
        Revisar([Fila(2, "A1", 500)], [p]);
        Assert.Equal(100m, p.Costo);
        Assert.Equal(200m, p.Precio);
    }

    [Fact]
    public void Empleado_no_puede_aplicar_aumento_por_proveedor()
    {
        Assert.False(Permisos.PuedeEditarProducto(Roles.Empleado));
        Assert.True(Permisos.PuedeEditarProducto(Roles.Dueno));
    }
}
