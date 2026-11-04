using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioLoginTests
{
    private static Usuario Usuario() => new() { NombreUsuario = "caja", HashClave = "x" };

    [Fact]
    public void El_quinto_fallo_frena_quince_minutos()
    {
        var usuario = Usuario();
        var ahora = new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);

        for (var i = 0; i < ServicioLogin.MaximoFallos - 1; i++)
            ServicioLogin.RegistrarFallo(usuario, ahora);

        Assert.False(ServicioLogin.EstaFrenado(usuario, ahora));
        ServicioLogin.RegistrarFallo(usuario, ahora);

        Assert.True(ServicioLogin.EstaFrenado(usuario, ahora.AddMinutes(14)));
        Assert.False(ServicioLogin.EstaFrenado(usuario, ahora.Add(ServicioLogin.Freno)));
        Assert.Equal(0, usuario.IntentosFallidos);
    }

    [Fact]
    public void Un_acierto_reinicia_el_freno()
    {
        var usuario = Usuario();
        var ahora = new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);
        for (var i = 0; i < ServicioLogin.MaximoFallos; i++)
            ServicioLogin.RegistrarFallo(usuario, ahora);

        ServicioLogin.RegistrarAcierto(usuario);

        Assert.False(ServicioLogin.EstaFrenado(usuario, ahora));
        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.BloqueadoHasta);
    }

    [Fact]
    public void Mientras_esta_frenado_no_suma_intentos()
    {
        var usuario = Usuario();
        var ahora = new DateTimeOffset(2026, 11, 1, 12, 0, 0, TimeSpan.Zero);
        for (var i = 0; i < ServicioLogin.MaximoFallos; i++)
            ServicioLogin.RegistrarFallo(usuario, ahora);
        var hasta = usuario.BloqueadoHasta;

        ServicioLogin.RegistrarFallo(usuario, ahora.AddMinutes(1));

        Assert.Equal(hasta, usuario.BloqueadoHasta);
        Assert.Equal(0, usuario.IntentosFallidos);
    }
}
