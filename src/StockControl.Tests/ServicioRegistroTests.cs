using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioRegistroTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 15, 12, 0, 0, TimeSpan.FromHours(-3));

    private static RegistroPedido Pedido(string modalidad = ServicioRegistro.Prueba) => new()
    {
        Nombre = "Jorge",
        Email = "Jorge@Kiosco.com ",
        NombreUsuario = "jorge",
        NombreLocal = "Lo de Jorge",
        Modalidad = modalidad
    };

    [Fact]
    public void Registro_con_prueba_crea_local_por_7_dias()
    {
        var (_, local) = ServicioRegistro.Armar(Pedido(), Ahora);

        Assert.Equal(ServicioAbono.Trial, local.EstadoAbono);
        Assert.Equal(Ahora.AddDays(7), local.Vence);
        Assert.True(ServicioAbono.Estado(local, Ahora).PuedeOperarCaja);
    }

    [Fact]
    public void Registro_para_pagar_crea_local_que_no_cobra()
    {
        var (_, local) = ServicioRegistro.Armar(Pedido(ServicioRegistro.Pagar), Ahora);

        Assert.Null(local.Vence);
        Assert.False(ServicioAbono.Estado(local, Ahora).PuedeOperarCaja);
    }

    [Fact]
    public void Registro_crea_al_usuario_como_dueno()
    {
        var (dueno, local) = ServicioRegistro.Armar(Pedido(), Ahora);

        Assert.Equal(Roles.Dueno, dueno.Rol);
        Assert.True(dueno.Activo);
        Assert.Equal("jorge@kiosco.com", dueno.Email);
        Assert.Equal("Lo de Jorge", local.Nombre);
    }

    [Theory]
    [InlineData("jorge@kiosco.com", "corta")]
    [InlineData("no-es-mail", "claveLarga1")]
    [InlineData("jorge@", "claveLarga1")]
    public void Clave_corta_o_mail_invalido_da_error(string email, string clave)
    {
        var pedido = Pedido();
        pedido.Email = email;

        Assert.Throws<ErrorNegocio>(() => ServicioRegistro.Validar(pedido, clave));
    }

    [Fact]
    public void Registro_valido_no_da_error()
    {
        ServicioRegistro.Validar(Pedido(), "claveLarga1");
    }
}
