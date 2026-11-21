using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioTerminosTiendaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 11, 20, 12, 0, 0, TimeSpan.FromHours(-3));

    private static Local Local() => new()
    {
        IdLocal = 1,
        Nombre = "Almagro",
        AliasTienda = "almagro",
        EstadoTienda = ServicioTienda.Inactiva
    };

    [Fact]
    public void Publicar_sin_aceptar_terminos_falla()
    {
        var local = Local();

        Assert.Throws<ErrorNegocio>(() =>
            ServicioTerminosTienda.ExigirAlPublicar(local, ServicioTienda.Activa, acepta: false, Ahora));
        Assert.Equal("", local.VersionTerminosTienda);
    }

    [Fact]
    public void Publicar_aceptando_marca_la_version()
    {
        var local = Local();

        ServicioTerminosTienda.ExigirAlPublicar(local, ServicioTienda.Activa, acepta: true, Ahora);

        Assert.Equal(ServicioTerminosTienda.Version, local.VersionTerminosTienda);
        Assert.Equal(Ahora, local.AceptoTerminosTiendaEn);
        Assert.True(ServicioTerminosTienda.Vigente(local));
    }

    [Fact]
    public void Si_ya_acepto_no_pide_de_nuevo()
    {
        var local = Local();
        ServicioTerminosTienda.Marcar(local, Ahora);

        ServicioTerminosTienda.ExigirAlPublicar(local, ServicioTienda.Activa, acepta: false, Ahora.AddDays(1));

        Assert.Equal(Ahora, local.AceptoTerminosTiendaEn);
    }

    [Fact]
    public void Pausar_o_apagar_no_exige_terminos()
    {
        var local = Local();

        ServicioTerminosTienda.ExigirAlPublicar(local, ServicioTienda.Pausada, acepta: false, Ahora);
        ServicioTerminosTienda.ExigirAlPublicar(local, ServicioTienda.Inactiva, acepta: false, Ahora);

        Assert.Equal("", local.VersionTerminosTienda);
    }
}
