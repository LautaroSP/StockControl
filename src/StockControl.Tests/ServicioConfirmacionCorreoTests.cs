using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioConfirmacionCorreoTests
{
    [Fact]
    public void Sin_clave_remitente_o_sitio_no_se_confirma_el_mail()
    {
        Assert.False(ServicioConfirmacionCorreo.HayQueConfirmar(null, "hola@dominio.com", "https://dominio.com"));
        Assert.False(ServicioConfirmacionCorreo.HayQueConfirmar("clave", " ", "https://dominio.com"));
        Assert.False(ServicioConfirmacionCorreo.HayQueConfirmar("clave", "hola@dominio.com", ""));
        Assert.True(ServicioConfirmacionCorreo.HayQueConfirmar("clave", "hola@dominio.com", "https://dominio.com"));
    }

    [Fact]
    public void El_enlace_valido_confirma_y_el_vencido_no()
    {
        var ahora = new DateTimeOffset(2026, 10, 22, 15, 0, 0, TimeSpan.Zero);
        var token = ServicioConfirmacionCorreo.NuevoToken();
        var hash = ServicioConfirmacionCorreo.Hash(token);
        var usuario = new Usuario { EmailConfirmado = false, TokenConfirmacion = hash, VenceConfirmacion = ahora.AddHours(1) };

        Assert.Null(ServicioConfirmacionCorreo.Rechazo(token, hash, usuario.VenceConfirmacion, ahora));
        Assert.Equal("El enlace venció. Registrate de nuevo.", ServicioConfirmacionCorreo.Rechazo(token, hash, ahora.AddMinutes(-1), ahora));
        Assert.Equal("El enlace no sirve. Registrate de nuevo.", ServicioConfirmacionCorreo.Rechazo("otro", hash, usuario.VenceConfirmacion, ahora));

        ServicioConfirmacionCorreo.Confirmar(usuario);

        Assert.True(usuario.EmailConfirmado);
        Assert.Null(usuario.TokenConfirmacion);
        Assert.Null(usuario.VenceConfirmacion);
    }
}
