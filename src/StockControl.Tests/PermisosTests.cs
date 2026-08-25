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
}
