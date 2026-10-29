using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioAvisosTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 28, 12, 0, 0, TimeSpan.FromHours(-3));

    private static readonly PedidoTienda Pedido = new()
    {
        Id = 7, IdLocal = 1, Numero = 12, Nombre = "Ana Pérez", Total = 3000
    };

    private static Usuario Usuario(int id, string rol = Roles.Dueno, bool activo = true) =>
        new() { Id = id, Rol = rol, Activo = activo, NombreUsuario = $"u{id}" };

    private static AvisosPedido Generar(
        IEnumerable<Usuario> usuarios, IEnumerable<UsuarioLocal> vinculos,
        IEnumerable<PreferenciaAviso>? preferencias = null, bool empleadoVePedidos = false, string tipo = ServicioAvisos.PedidoNuevo) =>
        ServicioAvisos.DePedido(tipo, Pedido, vinculos, usuarios, preferencias ?? [], empleadoVePedidos, Ahora);

    private static UsuarioLocal Vinculo(int idUsuario, int idLocal = 1) => new() { IdUsuario = idUsuario, IdLocal = idLocal };

    [Fact]
    public void El_aviso_llega_solo_a_usuarios_de_ese_local()
    {
        var avisos = Generar([Usuario(1), Usuario(2)], [Vinculo(1), Vinculo(2, idLocal: 2)]);

        var aviso = Assert.Single(avisos.Avisos);
        Assert.Equal(1, aviso.IdUsuario);
        Assert.Equal(1, aviso.IdLocal);
        Assert.Equal(7, aviso.IdPedidoTienda);
    }

    [Fact]
    public void El_empleado_sin_permiso_no_recibe_avisos()
    {
        var avisos = Generar([Usuario(1), Usuario(2, Roles.Empleado)], [Vinculo(1), Vinculo(2)]);

        Assert.Equal([1], avisos.Avisos.Select(a => a.IdUsuario));
    }

    [Fact]
    public void El_empleado_habilitado_recibe_avisos()
    {
        var avisos = Generar([Usuario(2, Roles.Empleado)], [Vinculo(2)], empleadoVePedidos: true);

        Assert.Single(avisos.Avisos);
    }

    [Fact]
    public void El_usuario_inactivo_no_recibe_avisos()
    {
        Assert.Empty(Generar([Usuario(1, activo: false)], [Vinculo(1)]).Avisos);
    }

    [Fact]
    public void Sin_preferencias_recibe_push()
    {
        Assert.Equal([1], Generar([Usuario(1)], [Vinculo(1)]).IdsUsuarioPush);
    }

    [Fact]
    public void Quien_apaga_el_push_igual_recibe_la_campanita()
    {
        var preferencias = new[] { new PreferenciaAviso { IdLocal = 1, IdUsuario = 1, PushNuevos = false } };

        var avisos = Generar([Usuario(1)], [Vinculo(1)], preferencias);

        Assert.Single(avisos.Avisos);
        Assert.Empty(avisos.IdsUsuarioPush);
    }

    [Fact]
    public void La_preferencia_de_otro_local_no_cuenta()
    {
        var preferencias = new[] { new PreferenciaAviso { IdLocal = 2, IdUsuario = 1, PushNuevos = false } };

        Assert.Equal([1], Generar([Usuario(1)], [Vinculo(1)], preferencias).IdsUsuarioPush);
    }

    [Fact]
    public void El_push_de_cancelados_se_elige_aparte()
    {
        var preferencias = new[] { new PreferenciaAviso { IdLocal = 1, IdUsuario = 1, PushCancelados = false } };

        Assert.Empty(Generar([Usuario(1)], [Vinculo(1)], preferencias, tipo: ServicioAvisos.PedidoCancelado).IdsUsuarioPush);
        Assert.Equal([1], Generar([Usuario(1)], [Vinculo(1)], preferencias).IdsUsuarioPush);
    }

    [Fact]
    public void El_texto_dice_numero_comprador_y_total()
    {
        var avisos = Generar([Usuario(1)], [Vinculo(1)]);

        Assert.Equal("N° 12 de Ana Pérez por $ 3.000,00", avisos.Texto);
        Assert.Equal(avisos.Texto, avisos.Avisos[0].Texto);
    }

    [Fact]
    public void Marcar_leido_guarda_la_primera_lectura()
    {
        var aviso = new Aviso();
        ServicioAvisos.MarcarLeido(aviso, Ahora);

        ServicioAvisos.MarcarLeido(aviso, Ahora.AddHours(1));

        Assert.Equal(Ahora, aviso.Leido);
    }
}
