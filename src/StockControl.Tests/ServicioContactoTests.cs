using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioContactoTests
{
    [Fact]
    public void Un_mensaje_valido_guarda_el_correo_en_minusculas()
    {
        var ahora = new DateTimeOffset(2026, 10, 20, 18, 0, 0, TimeSpan.Zero);
        var mensaje = ServicioContacto.Nuevo("Ana", "Ana@Mail.com", " Quiero la prueba ", ahora);

        Assert.Equal("Ana", mensaje.Nombre);
        Assert.Equal("ana@mail.com", mensaje.Correo);
        Assert.Equal("Quiero la prueba", mensaje.Mensaje);
        Assert.Equal(ahora, mensaje.Creado);
        Assert.Null(mensaje.Leido);
    }

    [Fact]
    public void Un_mensaje_incompleto_no_se_guarda()
    {
        var ahora = DateTimeOffset.UtcNow;
        Assert.Throws<ErrorNegocio>(() => ServicioContacto.Nuevo("Ana", "ana@mail.com", "  ", ahora));
    }

    [Fact]
    public void Marcar_leido_no_pisa_la_fecha_si_ya_estaba()
    {
        var leido = new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);
        var mensaje = new MensajeContacto { Leido = leido };

        ServicioContacto.MarcarLeido(mensaje, leido.AddHours(5));

        Assert.Equal(leido, mensaje.Leido);
    }

    [Fact]
    public void A_los_tres_dias_de_leido_el_mensaje_vence()
    {
        var leido = new DateTimeOffset(2026, 10, 20, 12, 0, 0, TimeSpan.Zero);

        Assert.False(ServicioContacto.YaVencio(leido, leido.AddDays(3).AddSeconds(-1)));
        Assert.True(ServicioContacto.YaVencio(leido, leido.AddDays(3)));
        Assert.False(ServicioContacto.YaVencio(null, leido.AddDays(10)));
    }

    [Fact]
    public void El_formulario_esta_activo_salvo_que_se_apague()
    {
        Assert.True(ServicioContacto.EstaActivo(null));
        Assert.True(ServicioContacto.EstaActivo("1"));
        Assert.False(ServicioContacto.EstaActivo("0"));
    }
}
