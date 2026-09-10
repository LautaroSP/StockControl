using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioGrupoTests
{
    private static GrupoProductos Grupo(int id = 1, decimal costo = 100, decimal precio = 200) =>
        new()
        {
            IdGrupoProducto = id,
            IdLocal = 1,
            NombreGrupo = "Gaseosas",
            Costo = costo,
            PrecioGrupo = precio
        };

    private static Producto Comun(int id, int grupo = 0) =>
        new()
        {
            Id = id,
            IdLocal = 1,
            Codigo = $"C{id}",
            Nombre = $"Prod {id}",
            Costo = 50,
            Precio = 80,
            IdGrupoProducto = grupo
        };

    [Fact]
    public void Asignar_aplica_precio_y_costo_del_grupo()
    {
        var g = Grupo();
        var p = Comun(1);
        ServicioGrupo.Asignar(p, g);
        Assert.Equal(1, p.IdGrupoProducto);
        Assert.Equal(100, p.Costo);
        Assert.Equal(200, p.Precio);
    }

    [Fact]
    public void Asignar_falla_si_es_sector()
    {
        var g = Grupo();
        var p = Comun(1);
        p.ProductoSector = true;
        Assert.Throws<ErrorNegocio>(() => ServicioGrupo.Asignar(p, g));
    }

    [Fact]
    public void AplicarAMiembros_actualiza_todos()
    {
        var g = Grupo(precio: 250, costo: 120);
        var a = Comun(1, grupo: 1);
        var b = Comun(2, grupo: 1);
        ServicioGrupo.AplicarAMiembros(g, [a, b]);
        Assert.Equal(250, a.Precio);
        Assert.Equal(120, a.Costo);
        Assert.Equal(250, b.Precio);
        Assert.Equal(120, b.Costo);
    }

    [Fact]
    public void AplicarAMiembros_actualiza_auditoria_de_todos()
    {
        var g = Grupo(precio: 250, costo: 120);
        var productos = new[] { Comun(1, grupo: 1), Comun(2, grupo: 1) };

        ServicioGrupo.AplicarAMiembros(g, productos, "matias");

        Assert.All(productos, p => Assert.Equal("matias", p.UsuarioModificacion));
        Assert.All(productos, p => Assert.NotNull(p.FechaModificacion));
    }

    [Fact]
    public void Sacar_deja_grupo_cero_sin_cambiar_precio()
    {
        var p = Comun(1, grupo: 1);
        p.Precio = 200;
        ServicioGrupo.Sacar(p);
        Assert.Equal(0, p.IdGrupoProducto);
        Assert.Equal(200, p.Precio);
    }

    [Fact]
    public void Eliminar_saca_a_todos_los_miembros()
    {
        var a = Comun(1, grupo: 1);
        var b = Comun(2, grupo: 1);
        ServicioGrupo.Eliminar([a, b]);
        Assert.Equal(0, a.IdGrupoProducto);
        Assert.Equal(0, b.IdGrupoProducto);
    }

    [Fact]
    public void ReemplazarMiembros_saca_y_asigna()
    {
        var g = Grupo();
        var a = Comun(1, grupo: 1);
        var b = Comun(2, grupo: 0);
        var c = Comun(3, grupo: 1);
        ServicioGrupo.ReemplazarMiembros(g, [a, b, c], [2, 3]);
        Assert.Equal(0, a.IdGrupoProducto);
        Assert.Equal(1, b.IdGrupoProducto);
        Assert.Equal(200, b.Precio);
        Assert.Equal(1, c.IdGrupoProducto);
    }
}
