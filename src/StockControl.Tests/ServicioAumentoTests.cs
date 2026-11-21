using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioAumentoTests
{
    static Producto Producto(int id, decimal precio, decimal costo, int grupo = 0, int? categoria = null, bool sector = false) => new()
    {
        Id = id,
        IdLocal = 1,
        Codigo = "A" + id,
        Nombre = "Producto " + id,
        Precio = precio,
        Costo = costo,
        IdGrupoProducto = grupo,
        IdCategoria = categoria,
        ProductoSector = sector
    };

    static Aumento Porcentaje(decimal valor) => new(TipoAumento.Porcentaje, valor);

    static Aumento Pesos(decimal valor) => new(TipoAumento.Importe, valor);

    [Fact]
    public void Porcentaje_de_precio_no_toca_el_costo_ni_el_precio_por_kilo()
    {
        var p = Producto(1, 1000, 400);
        p.PrecioPorKilo = 2500;
        var n = ServicioAumento.Aplicar([p], [], [1], Porcentaje(10), Aumento.Nada, "ana").Cantidad;
        Assert.Equal(1, n);
        Assert.Equal(1100m, p.Precio);
        Assert.Equal(400m, p.Costo);
        Assert.Equal(2500m, p.PrecioPorKilo);
        Assert.Equal("ana", p.UsuarioModificacion);
    }

    [Fact]
    public void Precio_y_costo_van_por_separado()
    {
        var p = Producto(1, 1000, 400);
        ServicioAumento.Aplicar([p], [], [1], Porcentaje(10), Pesos(50), "ana");
        Assert.Equal(1100m, p.Precio);
        Assert.Equal(450m, p.Costo);
    }

    [Fact]
    public void Importe_fijo_suma_pesos()
    {
        var p = Producto(1, 1000, 400);
        ServicioAumento.Aplicar([p], [], [1], Pesos(100), Aumento.Nada, "ana");
        Assert.Equal(1100m, p.Precio);
        Assert.Equal(400m, p.Costo);
    }

    [Fact]
    public void Categoria_efectiva_incluye_la_heredada_y_deja_afuera_la_otra()
    {
        var grupo = new GrupoProductos { IdGrupoProducto = 7, IdCategoria = 3 };
        var heredado = Producto(1, 500, 200, grupo: 7);
        var propio = Producto(2, 800, 300, categoria: 3);
        var otra = Producto(3, 100, 50, categoria: 4);
        var generico = Producto(4, 100, 50, categoria: 3);
        generico.Codigo = "GENERIC-1";
        var ids = ServicioAumento.IdsDeCategoria(3, [heredado, propio, otra, generico], [grupo]);
        Assert.Equal([1, 2], ids);
    }

    [Fact]
    public void IdsDeProveedor_trae_solo_productos_del_proveedor_sin_genericos()
    {
        var propio = Producto(1, 500, 200);
        propio.IdProveedor = 5;
        var otro = Producto(2, 500, 200);
        otro.IdProveedor = 6;
        var generico = Producto(3, 500, 200);
        generico.IdProveedor = 5;
        generico.Codigo = "GENERIC-1";
        Assert.Equal([1], ServicioAumento.IdsDeProveedor(5, [propio, otro, generico]));
    }

    [Fact]
    public void Aumento_por_proveedor_sube_precio_y_costo()
    {
        var a = Producto(1, 1000, 400);
        a.IdProveedor = 5;
        var b = Producto(2, 1000, 400);
        b.IdProveedor = 6;
        var ids = ServicioAumento.IdsDeProveedor(5, [a, b]);
        ServicioAumento.Aplicar([a, b], [], ids, Porcentaje(10), Porcentaje(8), "ana");
        Assert.Equal(1100m, a.Precio);
        Assert.Equal(432m, a.Costo);
        Assert.Equal(1000m, b.Precio);
    }

    [Fact]
    public void Aumentar_un_miembro_mueve_el_grupo_una_sola_vez()
    {
        var grupo = new GrupoProductos { IdGrupoProducto = 7, PrecioGrupo = 1000, Costo = 400 };
        var a = Producto(1, 1000, 400, grupo: 7);
        var b = Producto(2, 1000, 400, grupo: 7);
        var n = ServicioAumento.Aplicar([a, b], [grupo], [1, 2], Porcentaje(10), Pesos(100), "ana").Cantidad;
        Assert.Equal(2, n);
        Assert.Equal(1100m, grupo.PrecioGrupo);
        Assert.Equal(500m, grupo.Costo);
        Assert.Equal(1100m, a.Precio);
        Assert.Equal(1100m, b.Precio);
        Assert.Equal(500m, a.Costo);
        Assert.Equal(500m, b.Costo);
    }

    [Fact]
    public void Sector_sube_el_costo_y_deja_el_precio()
    {
        var p = Producto(1, 0, 400, sector: true);
        p.PrecioPorKilo = 800;
        ServicioAumento.Aplicar([p], [], [1], Porcentaje(10), Pesos(100), "ana");
        Assert.Equal(0m, p.Precio);
        Assert.Equal(500m, p.Costo);
        Assert.Equal(800m, p.PrecioPorKilo);
    }

    [Fact]
    public void Sector_solo_con_precio_no_cambia_nada()
    {
        var p = Producto(1, 0, 400, sector: true);
        var ex = Assert.Throws<ErrorNegocio>(() =>
            ServicioAumento.Aplicar([p], [], [1], Porcentaje(10), Aumento.Nada, "ana"));
        Assert.Contains("precio de lista", ex.Message);
        Assert.Equal(0m, p.Precio);
        Assert.Equal(400m, p.Costo);
    }

    [Fact]
    public void Sin_ajuste_rechaza()
    {
        var ex = Assert.Throws<ErrorNegocio>(() => ServicioAumento.ExigirAlguno(Aumento.Nada, Aumento.Nada));
        Assert.Contains("precio o de costo", ex.Message);
    }

    [Fact]
    public void Porcentaje_mayor_a_mil_rechaza()
    {
        var ex = Assert.Throws<ErrorNegocio>(() => ServicioAumento.Normalizar(TipoAumento.Porcentaje, 1001));
        Assert.Contains("1000", ex.Message);
    }

    [Fact]
    public void Redondea_a_dos_decimales()
    {
        var p = Producto(1, 333.33m, 0);
        ServicioAumento.Aplicar([p], [], [1], Porcentaje(10), Aumento.Nada, "ana");
        Assert.Equal(366.66m, p.Precio);
    }

    [Theory]
    [InlineData(1231, 10, 1240)]
    [InlineData(1231, 50, 1250)]
    [InlineData(1231, 100, 1300)]
    public void Redondear_sube_al_multiplo(decimal precio, int multiplo, decimal esperado)
    {
        Assert.Equal(esperado, ServicioAumento.Redondear(precio, multiplo));
    }

    [Fact]
    public void Redondear_deja_igual_un_precio_que_ya_es_multiplo()
    {
        Assert.Equal(1500m, ServicioAumento.Redondear(1500m, 50));
    }

    [Fact]
    public void Redondear_1996_50_a_100_da_2000()
    {
        Assert.Equal(2000m, ServicioAumento.Redondear(1996.50m, 100));
    }

    [Fact]
    public void Redondear_cero_no_redondea()
    {
        Assert.Equal(1996.50m, ServicioAumento.Redondear(1996.50m, 0));
    }

    [Fact]
    public void Redondeo_distinto_de_10_50_100_falla()
    {
        var ex = Assert.Throws<ErrorNegocio>(() => ServicioAumento.NormalizarRedondeo(25));
        Assert.Contains("10, 50 o 100", ex.Message);
    }

    [Fact]
    public void Aplicar_redondea_el_precio_y_no_el_costo()
    {
        var p = Producto(1, 1815, 403);
        ServicioAumento.Aplicar([p], [], [1], Porcentaje(10), Porcentaje(10), "ana", 100);
        Assert.Equal(2000m, p.Precio);
        Assert.Equal(443.30m, p.Costo);
    }

    [Fact]
    public void Aplicar_redondea_el_precio_del_grupo_y_lo_copia_a_los_miembros()
    {
        var grupo = new GrupoProductos { IdGrupoProducto = 7, PrecioGrupo = 1815, Costo = 400 };
        var a = Producto(1, 1815, 400, grupo: 7);
        var b = Producto(2, 1815, 400, grupo: 7);
        ServicioAumento.Aplicar([a, b], [grupo], [1, 2], Porcentaje(10), Aumento.Nada, "ana", 50);
        Assert.Equal(2000m, grupo.PrecioGrupo);
        Assert.Equal(2000m, a.Precio);
        Assert.Equal(2000m, b.Precio);
        Assert.Equal(400m, grupo.Costo);
    }

    [Fact]
    public void Aplicar_no_redondea_el_producto_sector()
    {
        var p = Producto(1, 0, 403, sector: true);
        ServicioAumento.Aplicar([p], [], [1], Aumento.Nada, Porcentaje(10), "ana", 100);
        Assert.Equal(0m, p.Precio);
        Assert.Equal(443.30m, p.Costo);
    }

    [Fact]
    public void Simular_no_modifica_productos_ni_grupos()
    {
        var grupo = new GrupoProductos { IdGrupoProducto = 7, PrecioGrupo = 1000, Costo = 400 };
        var a = Producto(1, 1000, 400, grupo: 7);
        var b = Producto(2, 500, 200);
        ServicioAumento.Simular([a, b], [grupo], [1, 2], Porcentaje(10), Pesos(50), 100);
        Assert.Equal(1000m, grupo.PrecioGrupo);
        Assert.Equal(400m, grupo.Costo);
        Assert.Equal(1000m, a.Precio);
        Assert.Equal(500m, b.Precio);
        Assert.Equal(200m, b.Costo);
        Assert.Equal("", b.UsuarioModificacion);
    }

    [Fact]
    public void Simular_da_el_mismo_resultado_que_aplicar()
    {
        var grupo = new GrupoProductos { IdGrupoProducto = 7, PrecioGrupo = 1815, Costo = 400 };
        var a = Producto(1, 1815, 400, grupo: 7);
        var b = Producto(2, 777, 200);
        var simulado = ServicioAumento.Simular([a, b], [grupo], [1, 2], Porcentaje(10), Pesos(50), 50);
        var aplicado = ServicioAumento.Aplicar([a, b], [grupo], [1, 2], Porcentaje(10), Pesos(50), "ana", 50);
        Assert.Equal(aplicado.Cantidad, simulado.Cantidad);
        Assert.Equal(aplicado.Cambios, simulado.Cambios);
    }

    [Fact]
    public void Simular_muestra_un_grupo_una_sola_vez()
    {
        var grupo = new GrupoProductos { IdGrupoProducto = 7, PrecioGrupo = 1000, Costo = 400 };
        var a = Producto(1, 1000, 400, grupo: 7);
        var b = Producto(2, 1000, 400, grupo: 7);
        var r = ServicioAumento.Simular([a, b], [grupo], [1, 2], Porcentaje(10), Aumento.Nada);
        Assert.Single(r.Cambios);
        Assert.Equal(2, r.Cantidad);
        Assert.Equal(7, r.Cambios[0].IdGrupoProducto);
    }

    [Fact]
    public void Aplicar_devuelve_valores_antes_y_despues_por_producto()
    {
        var p = Producto(1, 1000, 400);
        var c = ServicioAumento.Aplicar([p], [], [1], Porcentaje(10), Pesos(50), "ana").Cambios.Single();
        Assert.Equal(1, c.IdProducto);
        Assert.Equal(1000m, c.PrecioAntes);
        Assert.Equal(1100m, c.PrecioDespues);
        Assert.Equal(400m, c.CostoAntes);
        Assert.Equal(450m, c.CostoDespues);
    }

    [Fact]
    public void Aplicar_guarda_un_solo_item_por_grupo()
    {
        var grupo = new GrupoProductos { IdGrupoProducto = 7, PrecioGrupo = 1000, Costo = 400 };
        var a = Producto(1, 1000, 400, grupo: 7);
        var b = Producto(2, 1000, 400, grupo: 7);
        var r = ServicioAumento.Aplicar([a, b], [grupo], [1, 2], Porcentaje(10), Aumento.Nada, "ana");
        var historial = ServicioDeshacerAumento.Registrar(1, "ana", Porcentaje(10), Aumento.Nada, 0, r, DateTimeOffset.UtcNow);
        var item = Assert.Single(historial.Items);
        Assert.Equal(7, item.IdGrupoProducto);
        Assert.Null(item.IdProducto);
        Assert.Equal(1000m, item.PrecioAntes);
    }

    [Fact]
    public void Empleado_no_puede_aumentar_ni_deshacer()
    {
        Assert.False(Permisos.PuedeEditarProducto(Roles.Empleado));
        Assert.True(Permisos.PuedeEditarProducto(Roles.Dueno));
    }
}