using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioRecuperarClaveTests
{
    static readonly DateTimeOffset Ahora = new(2026, 11, 20, 18, 0, 0, TimeSpan.Zero);

    static Usuario Dueno() => new()
    {
        Id = 1,
        NombreUsuario = "jorge",
        Email = "jorge@mail.com",
        Rol = Roles.Dueno,
        VersionSesion = 3
    };

    [Fact]
    public void Dueno_recibe_un_token_que_vence_en_10_minutos_y_se_guarda_el_hash()
    {
        var u = Dueno();
        var token = ServicioRecuperarClave.Iniciar(u, Ahora);
        Assert.NotNull(token);
        Assert.NotEqual(token, u.TokenRecuperacion);
        Assert.Equal(ServicioConfirmacionCorreo.Hash(token!), u.TokenRecuperacion);
        Assert.Equal(Ahora.AddMinutes(10), u.VenceRecuperacion);
    }

    [Fact]
    public void Socio_tambien_puede()
    {
        var u = Dueno();
        u.Rol = Roles.Socio;
        Assert.NotNull(ServicioRecuperarClave.Iniciar(u, Ahora));
    }

    [Theory]
    [InlineData(Roles.Empleado)]
    [InlineData(Roles.Admin)]
    public void Empleado_y_admin_no_reciben_link(string rol)
    {
        var u = Dueno();
        u.Rol = rol;
        Assert.Null(ServicioRecuperarClave.Iniciar(u, Ahora));
        Assert.Null(u.TokenRecuperacion);
    }

    [Fact]
    public void Sin_mail_inactivo_o_sin_confirmar_no_recibe_link()
    {
        var sinMail = Dueno();
        sinMail.Email = "";
        var inactivo = Dueno();
        inactivo.Activo = false;
        var sinConfirmar = Dueno();
        sinConfirmar.EmailConfirmado = false;
        Assert.Null(ServicioRecuperarClave.Iniciar(sinMail, Ahora));
        Assert.Null(ServicioRecuperarClave.Iniciar(inactivo, Ahora));
        Assert.Null(ServicioRecuperarClave.Iniciar(sinConfirmar, Ahora));
        Assert.Null(ServicioRecuperarClave.Iniciar(null, Ahora));
    }

    [Fact]
    public void No_manda_otro_link_antes_de_un_minuto()
    {
        var u = Dueno();
        var primero = ServicioRecuperarClave.Iniciar(u, Ahora);
        Assert.Null(ServicioRecuperarClave.Iniciar(u, Ahora.AddSeconds(30)));
        Assert.Equal(ServicioConfirmacionCorreo.Hash(primero!), u.TokenRecuperacion);
        Assert.NotNull(ServicioRecuperarClave.Iniciar(u, Ahora.AddMinutes(1)));
    }

    [Fact]
    public void Link_vigente_se_acepta()
    {
        var u = Dueno();
        var token = ServicioRecuperarClave.Iniciar(u, Ahora);
        Assert.Null(ServicioRecuperarClave.Rechazo(token, u, Ahora.AddMinutes(9)));
    }

    [Fact]
    public void Link_vencido_se_rechaza()
    {
        var u = Dueno();
        var token = ServicioRecuperarClave.Iniciar(u, Ahora);
        Assert.Contains("venció", ServicioRecuperarClave.Rechazo(token, u, Ahora.AddMinutes(11)));
    }

    [Fact]
    public void Token_equivocado_se_rechaza()
    {
        var u = Dueno();
        ServicioRecuperarClave.Iniciar(u, Ahora);
        Assert.Contains("no sirve", ServicioRecuperarClave.Rechazo("otro", u, Ahora));
    }

    [Fact]
    public void Completar_borra_el_token_corta_sesiones_y_desbloquea()
    {
        var u = Dueno();
        u.IntentosFallidos = 5;
        u.BloqueadoHasta = Ahora.AddMinutes(15);
        var token = ServicioRecuperarClave.Iniciar(u, Ahora);
        ServicioRecuperarClave.Completar(u);
        Assert.Null(u.TokenRecuperacion);
        Assert.Null(u.VenceRecuperacion);
        Assert.Equal(4, u.VersionSesion);
        Assert.Equal(0, u.IntentosFallidos);
        Assert.Null(u.BloqueadoHasta);
        Assert.NotNull(ServicioRecuperarClave.Rechazo(token, u, Ahora));
    }

    [Fact]
    public void Clave_corta_se_rechaza()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioRecuperarClave.ExigirClave("1234567"));
        ServicioRecuperarClave.ExigirClave("12345678");
    }
}
