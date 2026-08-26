using StockControl.Dominio;

namespace StockControl.Tests;

public class PermisosTests
{
    [Fact]
    public void Empleado_no_crea_producto()
    {
        Assert.False(Permisos.PuedeCrearProducto(Roles.Empleado));
        Assert.True(Permisos.PuedeCrearProducto(Roles.Dueno));
    }

    [Fact]
    public void Empleado_no_cobra_al_costo()
    {
        Assert.False(Permisos.PuedeCobrarAlCosto(Roles.Empleado));
        Assert.True(Permisos.PuedeCobrarAlCosto(Roles.Dueno));
    }

    [Fact]
    public void Empleado_no_anula_venta()
    {
        Assert.False(Permisos.PuedeAnularVenta(Roles.Empleado));
        Assert.True(Permisos.PuedeAnularVenta(Roles.Dueno));
        Assert.False(Permisos.PuedeVerCostoEnInforme(Roles.Empleado));
        Assert.True(Permisos.PuedeVerCostoEnInforme(Roles.Dueno));
    }

    [Fact]
    public void Empleado_no_administra_grupos()
    {
        Assert.False(Permisos.PuedeAdministrarGrupos(Roles.Empleado));
        Assert.True(Permisos.PuedeAdministrarGrupos(Roles.Dueno));
    }
}
