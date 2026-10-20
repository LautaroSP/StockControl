using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioAbonoTests
{
    private static readonly TimeSpan Ar = TimeSpan.FromHours(-3);
    private static readonly DateTimeOffset Ahora = new(2026, 10, 10, 19, 45, 0, Ar);

    private static Local Local(DateTimeOffset? vence, string estado = ServicioAbono.AlDia) =>
        new() { IdLocal = 1, Nombre = "Almagro", EstadoAbono = estado, Vence = vence };

    [Fact]
    public void Pago_vence_el_mismo_dia_del_mes_siguiente_a_la_misma_hora()
    {
        var vence = ServicioAbono.VencePorPago(Ahora);

        Assert.Equal(new DateTimeOffset(2026, 11, 10, 19, 45, 0, Ar), vence);
    }

    [Fact]
    public void Pago_del_31_vence_el_ultimo_dia_del_mes_siguiente()
    {
        var vence = ServicioAbono.VencePorPago(new DateTimeOffset(2027, 1, 31, 10, 0, 0, Ar));

        Assert.Equal(new DateTimeOffset(2027, 2, 28, 10, 0, 0, Ar), vence);
    }

    [Fact]
    public void Pago_a_la_noche_no_corre_el_dia()
    {
        var vence = ServicioAbono.VencePorPago(new DateTimeOffset(2027, 1, 30, 22, 0, 0, Ar));

        Assert.Equal(new DateTimeOffset(2027, 2, 28, 22, 0, 0, Ar), vence);
    }

    [Fact]
    public void Faltando_5_dias_esta_por_vencer()
    {
        var estado = ServicioAbono.Estado(Local(Ahora.AddDays(5)), Ahora);

        Assert.Equal(ServicioAbono.PorVencer, estado.Estado);
        Assert.Equal(5, estado.DiasRestantes);
        Assert.True(estado.PuedeOperarCaja);
    }

    [Fact]
    public void Faltando_6_dias_esta_al_dia()
    {
        var estado = ServicioAbono.Estado(Local(Ahora.AddDays(6)), Ahora);

        Assert.Equal(ServicioAbono.AlDia, estado.Estado);
    }

    [Fact]
    public void Al_llegar_la_hora_del_vencimiento_no_opera_caja()
    {
        var estado = ServicioAbono.Estado(Local(Ahora), Ahora);

        Assert.Equal(ServicioAbono.Vencido, estado.Estado);
        Assert.False(estado.PuedeOperarCaja);
    }

    [Fact]
    public void Un_minuto_antes_del_vencimiento_opera_caja()
    {
        var estado = ServicioAbono.Estado(Local(Ahora.AddMinutes(1)), Ahora);

        Assert.True(estado.PuedeOperarCaja);
    }

    [Fact]
    public void Trial_vigente_muestra_dias_restantes()
    {
        var inicio = Ahora.AddDays(-2);
        var estado = ServicioAbono.Estado(Local(ServicioAbono.VenceTrial(inicio), ServicioAbono.Trial), Ahora);

        Assert.Equal(ServicioAbono.Trial, estado.Estado);
        Assert.Equal(5, estado.DiasRestantes);
        Assert.True(estado.PuedeOperarCaja);
    }

    [Fact]
    public void Trial_vencido_no_opera_caja()
    {
        var inicio = Ahora.AddDays(-8);
        var estado = ServicioAbono.Estado(Local(ServicioAbono.VenceTrial(inicio), ServicioAbono.Trial), Ahora);

        Assert.Equal(ServicioAbono.Vencido, estado.Estado);
        Assert.False(estado.PuedeOperarCaja);
    }

    [Fact]
    public void Local_sin_vencimiento_cuenta_como_vencido()
    {
        var estado = ServicioAbono.Estado(Local(null), Ahora);

        Assert.Equal(ServicioAbono.Vencido, estado.Estado);
        Assert.False(estado.PuedeOperarCaja);
    }

    [Fact]
    public void Total_mensual_es_locales_por_14999()
    {
        Assert.Equal(44_997m, ServicioAbono.TotalMensual(3));
    }

    private static Local ConDebito(DateTimeOffset? vence, string debito = ServicioAbono.DebitoActivo)
    {
        var local = Local(vence);
        local.EstadoSuscripcion = debito;
        return local;
    }

    [Fact]
    public void Renovar_antes_de_vencer_suma_un_mes_desde_el_vencimiento()
    {
        var vence = Ahora.AddDays(3);

        var nuevo = ServicioAbono.VencePorRenovacion(Local(vence), Ahora);

        Assert.Equal(ServicioAbono.VencePorPago(vence), nuevo);
    }

    [Fact]
    public void Renovar_vencido_cuenta_desde_el_pago()
    {
        var nuevo = ServicioAbono.VencePorRenovacion(Local(Ahora.AddDays(-2)), Ahora);

        Assert.Equal(ServicioAbono.VencePorPago(Ahora), nuevo);
    }

    [Fact]
    public void Pagar_en_trial_corta_el_trial_y_cuenta_desde_el_pago()
    {
        var nuevo = ServicioAbono.VencePorRenovacion(Local(Ahora.AddDays(4), ServicioAbono.Trial), Ahora);

        Assert.Equal(ServicioAbono.VencePorPago(Ahora), nuevo);
    }

    [Fact]
    public void Acreditar_deja_el_local_al_dia_y_guarda_vencimientos()
    {
        var vence = Ahora.AddDays(-1);
        var local = Local(vence);
        var abono = ServicioAbono.NuevoAbono(local.IdLocal, Abono.OrigenMp, Ahora);

        ServicioAbono.Acreditar(local, abono, Ahora, "123");

        Assert.Equal(Abono.Aprobado, abono.Estado);
        Assert.Equal("123", abono.IdPagoMp);
        Assert.Equal(vence, abono.VenceAnterior);
        Assert.Equal(ServicioAbono.VencePorPago(Ahora), abono.VenceNuevo);
        Assert.Equal(ServicioAbono.AlDia, local.EstadoAbono);
        Assert.Equal(abono.VenceNuevo, local.Vence);
    }

    [Fact]
    public void Abono_ya_aprobado_no_se_acredita_dos_veces()
    {
        var local = Local(Ahora.AddDays(-1));
        var abono = ServicioAbono.NuevoAbono(local.IdLocal, Abono.OrigenMp, Ahora);
        ServicioAbono.Acreditar(local, abono, Ahora, "123");
        var venceTrasPrimerPago = local.Vence;

        Assert.Throws<ErrorNegocio>(() => ServicioAbono.Acreditar(local, abono, Ahora, "123"));
        Assert.Equal(venceTrasPrimerPago, local.Vence);
    }

    [Theory]
    [InlineData("rejected", "ARS", 14999)]
    [InlineData("approved", "USD", 14999)]
    [InlineData("approved", "ARS", 100)]
    public void Pago_no_aprobado_o_con_otro_importe_no_activa(string estado, string moneda, decimal importe)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioAbono.ValidarPagoMp(estado, moneda, importe));
    }

    [Fact]
    public void Debito_activo_da_3_dias_de_gracia_despues_del_vencimiento()
    {
        var local = ConDebito(Ahora.AddDays(-2));

        var estado = ServicioAbono.Estado(local, Ahora);

        Assert.True(estado.PuedeOperarCaja);
        Assert.True(estado.EnGracia);
        Assert.Equal(1, estado.DiasRestantes);
        Assert.False(ServicioAbono.Estado(local, Ahora.AddDays(1)).PuedeOperarCaja);
    }

    [Theory]
    [InlineData(ServicioAbono.DebitoPausado)]
    [InlineData(ServicioAbono.DebitoCancelado)]
    public void Debito_pausado_o_cancelado_no_da_gracia(string debito)
    {
        var estado = ServicioAbono.Estado(ConDebito(Ahora.AddMinutes(-1), debito), Ahora);

        Assert.Equal(ServicioAbono.Vencido, estado.Estado);
        Assert.False(estado.PuedeOperarCaja);
    }

    [Fact]
    public void Cobro_del_debito_en_la_gracia_renueva_desde_el_vencimiento()
    {
        var vence = Ahora.AddDays(-2);

        var nuevo = ServicioAbono.VencePorRenovacion(ConDebito(vence), Ahora);

        Assert.Equal(ServicioAbono.VencePorPago(vence), nuevo);
    }

    [Fact]
    public void Debito_activo_no_deja_pagar_este_mes()
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioAbono.ValidarPagoManual(ConDebito(Ahora.AddDays(10))));

        Assert.Equal("Cancelá el débito automático para pagar este mes.", error.Message);
    }

    [Fact]
    public void Sin_debito_activo_se_puede_pagar_este_mes()
    {
        var local = ConDebito(Ahora.AddDays(10), ServicioAbono.DebitoCancelado);

        ServicioAbono.ValidarPagoManual(local);
    }

    [Fact]
    public void Debito_de_local_al_dia_arranca_en_el_vencimiento()
    {
        var vence = Ahora.AddDays(10);

        Assert.Equal(vence, ServicioAbono.InicioDebito(Local(vence), Ahora));
    }

    [Fact]
    public void Debito_de_local_vencido_o_en_prueba_arranca_ya()
    {
        Assert.Equal(Ahora, ServicioAbono.InicioDebito(Local(Ahora.AddDays(-1)), Ahora));
        Assert.Equal(Ahora, ServicioAbono.InicioDebito(Local(Ahora.AddDays(4), ServicioAbono.Trial), Ahora));
    }

    [Theory]
    [InlineData("pending", ServicioAbono.DebitoPendiente)]
    [InlineData("authorized", ServicioAbono.DebitoActivo)]
    [InlineData("paused", ServicioAbono.DebitoPausado)]
    [InlineData("cancelled", ServicioAbono.DebitoCancelado)]
    public void Estado_de_suscripcion_de_mp_se_traduce(string estadoMp, string esperado)
    {
        Assert.Equal(esperado, ServicioAbono.EstadoSuscripcionMp(estadoMp));
    }

    [Fact]
    public void Estado_de_suscripcion_desconocido_da_error()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioAbono.EstadoSuscripcionMp("raro"));
    }
}
