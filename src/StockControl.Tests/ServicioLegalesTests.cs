using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioLegalesTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 11, 11, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Version_vieja_obliga_a_reaceptar()
    {
        var dueno = new Usuario { Rol = Roles.Dueno, VersionTerminos = "" };

        Assert.True(ServicioTerminos.DebeAceptar(dueno.Rol));
        Assert.False(ServicioTerminos.Vigente(dueno));

        ServicioTerminos.Marcar(dueno, Ahora);

        Assert.True(ServicioTerminos.Vigente(dueno));
        Assert.Equal(ServicioTerminos.Version, dueno.VersionTerminos);
        Assert.Equal(Ahora, dueno.AceptoTerminosEn);
    }

    [Fact]
    public void El_empleado_no_acepta_terminos()
    {
        Assert.False(ServicioTerminos.DebeAceptar(Roles.Empleado));
    }

    [Fact]
    public void Arrepentimiento_da_codigo_y_queda_pendiente()
    {
        var pedido = ServicioArrepentimiento.Nuevo("Ana", "Ana@Kiosco.com", "Lo de Ana", "Me arrepiento del abono", Ahora);

        Assert.Equal(8, pedido.Codigo.Length);
        Assert.Equal(Arrepentimiento.Pendiente, pedido.Estado);
        Assert.Null(pedido.IdLocal);
        Assert.Equal("ana@kiosco.com", pedido.Correo);
    }

    [Fact]
    public void Baja_deja_el_local_sin_renovar()
    {
        var vence = Ahora.AddDays(10);
        var local = new Local
        {
            Vence = vence,
            EstadoSuscripcion = ServicioAbono.DebitoActivo,
            Renueva = true
        };

        ServicioAbono.DarDeBaja(local);

        Assert.False(local.Renueva);
        Assert.False(ServicioAbono.RenuevaConDebito(local));
        Assert.Equal(vence, local.Vence);
        Assert.Equal(ServicioAbono.DebitoCancelado, local.EstadoSuscripcion);
    }
}
