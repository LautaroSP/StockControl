namespace StockControl.Dominio;

public sealed class EstadoAbonoLocal
{
    public required string Estado { get; init; }
    public DateTimeOffset? Vence { get; init; }
    public DateTimeOffset? Limite { get; init; }
    public int DiasRestantes { get; init; }
    public bool PuedeOperarCaja { get; init; }
    public bool EnGracia { get; init; }
}

public static class ServicioAbono
{
    public const decimal PrecioMensual = 14_999m;
    public const int DiasTrial = 7;
    public const int DiasAviso = 5;
    public const int DiasGracia = 3;

    public const string Pendiente = "pendiente";
    public const string Trial = "trial";
    public const string AlDia = "al_dia";
    public const string PorVencer = "por_vencer";
    public const string Vencido = "vencido";

    public const string SinDebito = "";
    public const string DebitoPendiente = "pendiente";
    public const string DebitoActivo = "activa";
    public const string DebitoPausado = "pausada";
    public const string DebitoCancelado = "cancelada";

    /// <summary>Mismo día del mes siguiente, a la misma hora argentina. Si ese día no existe, el último del mes.</summary>
    public static DateTimeOffset VencePorPago(DateTimeOffset pago)
    {
        var local = TimeZoneInfo.ConvertTime(pago, DiaArgentina.Zona).DateTime.AddMonths(1);
        return new DateTimeOffset(local, DiaArgentina.Zona.GetUtcOffset(local)).ToUniversalTime();
    }

    public static DateTimeOffset VenceTrial(DateTimeOffset inicio) => inicio.AddDays(DiasTrial);

    /// <summary>Hasta cuándo puede cobrar: el vencimiento, más la gracia si tiene débito activo.</summary>
    public static DateTimeOffset? Limite(Local local) =>
        local.Vence is { } vence && local.EstadoSuscripcion == DebitoActivo ? vence.AddDays(DiasGracia) : local.Vence;

    public static EstadoAbonoLocal Estado(Local local, DateTimeOffset ahora)
    {
        if (local.EstadoAbono == Pendiente)
            return new EstadoAbonoLocal { Estado = Pendiente, Vence = null, Limite = null, DiasRestantes = 0, PuedeOperarCaja = false };

        if (Limite(local) is not { } limite || ahora >= limite)
            return new EstadoAbonoLocal { Estado = Vencido, Vence = local.Vence, Limite = Limite(local), DiasRestantes = 0, PuedeOperarCaja = false };

        var vence = local.Vence!.Value;
        var dias = (int)Math.Ceiling((limite - ahora).TotalDays);
        if (ahora >= vence)
            return new EstadoAbonoLocal { Estado = PorVencer, Vence = vence, Limite = limite, DiasRestantes = dias, PuedeOperarCaja = true, EnGracia = true };

        var estado = local.EstadoAbono == Trial ? Trial
            : dias <= DiasAviso ? PorVencer
            : AlDia;
        return new EstadoAbonoLocal { Estado = estado, Vence = vence, Limite = limite, DiasRestantes = dias, PuedeOperarCaja = true };
    }

    public static decimal TotalMensual(int cantidadLocales) => cantidadLocales * PrecioMensual;

    /// <summary>Trial, pendiente y vencido no cuentan como cobrados. Al día y por vencer sí.</summary>
    public static bool CuentaEnElMes(string estado) => estado is AlDia or PorVencer;

    public static decimal TotalDelMes(IEnumerable<string> estados) =>
        TotalMensual(estados.Count(CuentaEnElMes));

    /// <summary>El mes nuevo arranca desde el vencimiento si sigue vigente (o en gracia); si venció o está en prueba, desde el pago.</summary>
    public static DateTimeOffset VencePorRenovacion(Local local, DateTimeOffset pago)
    {
        var vigente = local.EstadoAbono != Trial && Limite(local) is { } limite && limite > pago;
        return VencePorPago(vigente ? local.Vence!.Value : pago);
    }

    public static Abono NuevoAbono(int idLocal, string origen, DateTimeOffset ahora) => new()
    {
        IdLocal = idLocal,
        Importe = PrecioMensual,
        Estado = Abono.Pendiente,
        Origen = origen,
        FechaCreacion = ahora
    };

    public static void ValidarPagoManual(Local local)
    {
        if (local.EstadoSuscripcion == DebitoActivo)
            throw new ErrorNegocio("Cancelá el débito automático para pagar este mes.");
    }

    public static Abono RegistrarPagoAdmin(Local local, DateTimeOffset ahora, string canal)
    {
        ValidarPagoManual(local);
        var abono = NuevoAbono(local.IdLocal, Abono.OrigenAdmin, ahora);
        abono.Canal = ServicioAdmin.ValidarCanal(canal);
        abono.EstadoAnterior = local.EstadoAbono;
        abono.VenceAnterior = local.Vence;
        abono.VenceNuevo = VencePorRenovacion(local, ahora);
        abono.Estado = Abono.Aprobado;
        abono.FechaAcreditacion = ahora;
        local.EstadoAbono = AlDia;
        local.Vence = abono.VenceNuevo;
        return abono;
    }

    public static void AnularPagoAdmin(Local local, Abono? abono)
    {
        ValidarPagoManual(local);
        if (abono == null || abono.IdLocal != local.IdLocal || abono.Estado != Abono.Aprobado)
            throw new ErrorNegocio("No hay un pago para anular.");
        if (abono.Origen != Abono.OrigenAdmin)
            throw new ErrorNegocio("Ese pago lo hizo Mercado Pago.");
        if (local.Vence != abono.VenceNuevo)
            throw new ErrorNegocio("Ese pago ya no es el vigente.");

        abono.Estado = Abono.Anulado;
        local.Vence = abono.VenceAnterior;
        local.EstadoAbono = string.IsNullOrEmpty(abono.EstadoAnterior) ? AlDia : abono.EstadoAnterior;
    }

    public static void ValidarPagoMp(string estado, string moneda, decimal importe)
    {
        if (estado != "approved")
            throw new ErrorNegocio("El pago no está aprobado.");
        if (moneda != "ARS" || importe != PrecioMensual)
            throw new ErrorNegocio("El importe del pago no coincide con el abono.");
    }

    public static void Acreditar(Local local, Abono abono, DateTimeOffset fechaPago, string idPagoMp)
    {
        if (abono.Estado != Abono.Pendiente)
            throw new ErrorNegocio("El abono ya fue procesado.");
        if (abono.IdLocal != local.IdLocal)
            throw new ErrorNegocio("El abono es de otro local.");

        abono.VenceAnterior = local.Vence;
        abono.VenceNuevo = VencePorRenovacion(local, fechaPago);
        abono.Estado = Abono.Aprobado;
        abono.FechaAcreditacion = fechaPago;
        abono.IdPagoMp = idPagoMp;
        local.EstadoAbono = AlDia;
        local.Vence = abono.VenceNuevo;
    }

    /// <summary>Primer cobro del débito: al vencer lo ya pagado, o ya si está vencido o en prueba.</summary>
    public static DateTimeOffset InicioDebito(Local local, DateTimeOffset ahora) =>
        local.EstadoAbono == AlDia && local.Vence is { } vence && vence > ahora ? vence : ahora;

    public static string EstadoSuscripcionMp(string estadoMp) => estadoMp switch
    {
        "pending" => DebitoPendiente,
        "authorized" => DebitoActivo,
        "paused" => DebitoPausado,
        "cancelled" => DebitoCancelado,
        _ => throw new ErrorNegocio($"Estado de suscripción desconocido: {estadoMp}.")
    };
}
