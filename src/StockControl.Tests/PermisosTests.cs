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

    [Fact]
    public void Empleado_no_administra_medios_de_pago()
    {
        Assert.False(Permisos.PuedeAdministrarMetodosPago(Roles.Empleado));
        Assert.True(Permisos.PuedeAdministrarMetodosPago(Roles.Dueno));
    }

    [Fact]
    public void Socio_tiene_permisos_operativos_pero_no_asigna_socios()
    {
        Assert.True(Permisos.EsDuenoOperativo(Roles.Socio));
        Assert.True(Permisos.PuedeAdministrarUsuarios(Roles.Socio));
        Assert.False(Permisos.PuedeAsignarSocio(Roles.Socio));
        Assert.True(Permisos.PuedeAsignarSocio(Roles.Dueno));
    }

    [Fact]
    public void Admin_no_modifica_configuracion_del_local()
    {
        Assert.False(Permisos.PuedeConfigurarLocal(Roles.Admin));
        Assert.True(Permisos.PuedeConfigurarLocal(Roles.Dueno));
        Assert.True(Permisos.PuedeConfigurarLocal(Roles.Socio));
    }

    [Fact]
    public void Empleado_modifica_precio_solo_si_el_local_lo_habilita()
    {
        Assert.False(Permisos.PuedeModificarPrecioEmpleado(Roles.Empleado, false));
        Assert.True(Permisos.PuedeModificarPrecioEmpleado(Roles.Empleado, true));
        Assert.False(Permisos.PuedeModificarPrecioEmpleado(Roles.Socio, true));
    }
}
