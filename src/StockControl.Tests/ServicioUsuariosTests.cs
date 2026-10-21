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
}
