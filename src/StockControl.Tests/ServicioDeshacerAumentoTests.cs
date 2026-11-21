using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioDeshacerAumentoTests
{
    static Producto Producto(int id, decimal precio, decimal costo, int grupo = 0) => new()
    {
        Id = id,
        IdLocal = 1,
        Codigo = "A" + id,
        Nombre = "Producto " + id,
        Precio = precio,
        Costo = costo,
        IdGrupoProducto = grupo
    };

    static GrupoProductos Grupo(decimal precio, decimal costo) =>
        new() { IdGrupoProducto = 7, NombreGrupo = "Grupo 7", PrecioGrupo = precio, Costo = costo };

    static Aumento Porcentaje(decimal valor) => new(TipoAumento.Porcentaje, valor);

    static AumentoHistorial Aumentar(IReadOnlyList<Producto> productos, IReadOnlyList<GrupoProductos> grupos, DateTimeOffset? fecha = null)
    {
        var ids = productos.Select(p => p.Id).ToList();
        var r = ServicioAumento.Aplicar(productos, grupos, ids, Porcentaje(10), Porcentaje(10), "ana");
        return ServicioDeshacerAumento.Registrar(1, "ana", Porcentaje(10), Porcentaje(10), 0, r, fecha ?? DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Deshacer_vuelve_precio_y_costo_anteriores()
    {
        var p = Producto(1, 1000, 400);
        var h = Aumentar([p], []);
        var r = ServicioDeshacerAumento.Deshacer([h], [p], [], "luis");
        Assert.Equal(1, r.Restaurados);
        Assert.Empty(r.Omitidos);
        Assert.Equal(1000m, p.Precio);
        Assert.Equal(400m, p.Costo);
        Assert.Equal("luis", p.UsuarioModificacion);
    }

    [Fact]
    public void Deshacer_vuelve_el_grupo_y_sus_miembros()
    {
        var g = Grupo(1000, 400);
        var a = Producto(1, 1000, 400, grupo: 7);
        var b = Producto(2, 1000, 400, grupo: 7);
        var h = Aumentar([a, b], [g]);
        ServicioDeshacerAumento.Deshacer([h], [a, b], [g], "luis");
        Assert.Equal(1000m, g.PrecioGrupo);
        Assert.Equal(400m, g.Costo);
        Assert.Equal(1000m, a.Precio);
        Assert.Equal(400m, b.Costo);
    }

    [Fact]
    public void Deshacer_no_pisa_un_producto_tocado_despues_y_lo_lista()
    {
        var p = Producto(1, 1000, 400);
        var h = Aumentar([p], []);
        p.Precio = 1234;
        var r = ServicioDeshacerAumento.Deshacer([h], [p], [], "luis");
        Assert.Equal(0, r.Restaurados);
        Assert.Equal(["Producto 1"], r.Omitidos);
        Assert.Equal(1234m, p.Precio);
    }

    [Fact]
    public void Deshacer_no_pisa_un_grupo_tocado_despues()
    {
        var g = Grupo(1000, 400);
        var a = Producto(1, 1000, 400, grupo: 7);
        var h = Aumentar([a], [g]);
        g.PrecioGrupo = 1500;
        a.Precio = 1500;
        var r = ServicioDeshacerAumento.Deshacer([h], [a], [g], "luis");
        Assert.Equal(["Grupo 7"], r.Omitidos);
        Assert.Equal(1500m, g.PrecioGrupo);
        Assert.Equal(1500m, a.Precio);
    }

    [Fact]
    public void Deshacer_restaura_los_no_tocados_aunque_otros_hayan_cambiado()
    {
        var a = Producto(1, 1000, 400);
        var b = Producto(2, 500, 200);
        var h = Aumentar([a, b], []);
        b.Costo = 999;
        var r = ServicioDeshacerAumento.Deshacer([h], [a, b], [], "luis");
        Assert.Equal(1, r.Restaurados);
        Assert.Equal(1000m, a.Precio);
        Assert.Equal(550m, b.Precio);
        Assert.Equal(999m, b.Costo);
    }

    [Fact]
    public void Deshacer_marca_el_aumento_como_deshecho()
    {
        var p = Producto(1, 1000, 400);
        var h = Aumentar([p], []);
        ServicioDeshacerAumento.Deshacer([h], [p], [], "luis");
        Assert.True(h.Deshecho);
        Assert.NotNull(h.FechaDeshecho);
        Assert.Equal("luis", h.UsuarioDeshizo);
    }

    [Fact]
    public void Deshacer_dos_veces_falla()
    {
        var p = Producto(1, 1000, 400);
        var h = Aumentar([p], []);
        ServicioDeshacerAumento.Deshacer([h], [p], [], "luis");
        var ex = Assert.Throws<ErrorNegocio>(() => ServicioDeshacerAumento.Deshacer([h], [p], [], "luis"));
        Assert.Contains("ya se deshizo", ex.Message);
    }

    [Fact]
    public void Deshacer_sin_aumentos_falla()
    {
        var ex = Assert.Throws<ErrorNegocio>(() => ServicioDeshacerAumento.Deshacer([], [], [], "luis"));
        Assert.Contains("No hay aumentos", ex.Message);
    }

    [Fact]
    public void Deshacer_solo_actua_sobre_el_ultimo_aumento()
    {
        var p = Producto(1, 1000, 400);
        var viejo = Aumentar([p], [], DateTimeOffset.UtcNow.AddDays(-1));
        var nuevo = Aumentar([p], [], DateTimeOffset.UtcNow);
        ServicioDeshacerAumento.Deshacer([viejo, nuevo], [p], [], "luis");
        Assert.True(nuevo.Deshecho);
        Assert.False(viejo.Deshecho);
        Assert.Equal(1100m, p.Precio);
        Assert.Equal(440m, p.Costo);
    }
}
