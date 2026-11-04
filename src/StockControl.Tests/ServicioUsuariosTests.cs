using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioUsuariosTests
{
    [Fact]
    public void El_dueno_ve_empleados_de_todos_sus_locales()
    {
        var vinculos = new (int IdUsuario, int IdLocal)[]
        {
            (1, 10), (1, 20),
            (2, 10),
            (3, 20)
        };
        var usuarios = new (int Id, string Rol)[]
        {
            (1, Roles.Dueno),
            (2, Roles.Empleado),
            (3, Roles.Socio)
        };

        var locales = ServicioUsuarios.LocalesDeLaCuenta(1, Roles.Dueno, vinculos, usuarios);
        var visibles = ServicioUsuarios.IdsVisibles(locales, vinculos, usuarios);

        Assert.Equal(new[] { 10, 20 }, locales.OrderBy(x => x));
        Assert.Equal(new[] { 2, 3 }, visibles.OrderBy(x => x));
    }

    [Fact]
    public void El_socio_ve_la_cuenta_del_dueno_aunque_este_en_un_solo_local()
    {
        var vinculos = new (int IdUsuario, int IdLocal)[]
        {
            (1, 10), (1, 20),
            (2, 10),
            (3, 20)
        };
        var usuarios = new (int Id, string Rol)[]
        {
            (1, Roles.Dueno),
            (2, Roles.Socio),
            (3, Roles.Empleado)
        };

        var locales = ServicioUsuarios.LocalesDeLaCuenta(2, Roles.Socio, vinculos, usuarios);
        var visibles = ServicioUsuarios.IdsVisibles(locales, vinculos, usuarios);

        Assert.Equal(new[] { 10, 20 }, locales.OrderBy(x => x));
        Assert.Contains(3, visibles);
        Assert.Contains(2, visibles);
    }

    [Fact]
    public void No_ve_usuarios_de_otra_cuenta()
    {
        var vinculos = new (int IdUsuario, int IdLocal)[]
        {
            (1, 10),
            (2, 10),
            (8, 80),
            (9, 80)
        };
        var usuarios = new (int Id, string Rol)[]
        {
            (1, Roles.Dueno),
            (2, Roles.Empleado),
            (8, Roles.Dueno),
            (9, Roles.Empleado)
        };

        var locales = ServicioUsuarios.LocalesDeLaCuenta(1, Roles.Dueno, vinculos, usuarios);
        var visibles = ServicioUsuarios.IdsVisibles(locales, vinculos, usuarios);

        Assert.Equal(new[] { 10 }, locales);
        Assert.Equal(new[] { 2 }, visibles);
    }

    [Fact]
    public void Un_token_con_version_vieja_o_sin_version_no_esta_vigente()
    {
        Assert.False(ServicioUsuarios.SesionVigente(1, 2));
        Assert.False(ServicioUsuarios.SesionVigente(null, 1));
        Assert.True(ServicioUsuarios.SesionVigente(2, 2));
    }

    [Fact]
    public void Cambiar_el_rol_sube_la_version()
    {
        var usuario = new Usuario { Rol = Roles.Socio, VersionSesion = 3 };

        ServicioUsuarios.CambiarRol(usuario, Roles.Empleado);

        Assert.Equal(Roles.Empleado, usuario.Rol);
        Assert.Equal(4, usuario.VersionSesion);
    }

    [Fact]
    public void El_mismo_rol_no_sube_la_version()
    {
        var usuario = new Usuario { Rol = Roles.Empleado, VersionSesion = 3 };

        ServicioUsuarios.CambiarRol(usuario, Roles.Empleado);

        Assert.Equal(3, usuario.VersionSesion);
    }

    [Fact]
    public void Cambiar_la_clave_sube_la_version()
    {
        var usuario = new Usuario { VersionSesion = 2 };

        ServicioUsuarios.ClaveCambiada(usuario);

        Assert.Equal(3, usuario.VersionSesion);
    }

    [Fact]
    public void Desactivar_sube_la_version_y_otra_vez_no()
    {
        var usuario = new Usuario { Activo = true, VersionSesion = 1 };

        ServicioUsuarios.Desactivar(usuario);
        ServicioUsuarios.Desactivar(usuario);

        Assert.False(usuario.Activo);
        Assert.Equal(2, usuario.VersionSesion);
    }

    [Fact]
    public void Sacar_de_un_local_pide_cortar_la_sesion()
    {
        Assert.True(ServicioUsuarios.SacoAlgunLocal([10, 20], [10]));
        Assert.False(ServicioUsuarios.SacoAlgunLocal([10], [10, 20]));
    }

    [Fact]
    public void Cerrar_sesiones_sube_la_version_sin_otro_cambio()
    {
        var usuario = new Usuario { Rol = Roles.Empleado, Activo = true, VersionSesion = 5 };

        ServicioUsuarios.CortarSesion(usuario);

        Assert.Equal(Roles.Empleado, usuario.Rol);
        Assert.True(usuario.Activo);
        Assert.Equal(6, usuario.VersionSesion);
    }
}
