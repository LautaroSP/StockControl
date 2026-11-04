namespace StockControl.Dominio;

public record DesgloseGestion(decimal Subtotal, decimal Gestion, bool ACargoComprador, decimal Total);

/// <summary>Gestión de la plataforma por pedido de la tienda. No depende del tamaño del carrito.</summary>
public static class ServicioGestionTienda
{
    public const decimal ImportePorPedido = 1_000m;
    public const int DiasParaPagar = 7;

    /// <summary>Si el subtotal es menor que el importe, la gestión es el subtotal.</summary>
    public static DesgloseGestion Desglose(Local local, decimal subtotal)
    {
        var gestion = local.ExentoGestion ? 0 : Math.Min(ImportePorPedido, Math.Max(0, subtotal));
        var total = local.GestionACargoComprador ? subtotal + gestion : subtotal;
        return new DesgloseGestion(subtotal, gestion, local.GestionACargoComprador, total);
    }

    /// <summary>Pagado por MP y no reembolsado: es lo que MP separó como marketplace_fee.</summary>
    public static decimal Cobrada(IEnumerable<PedidoTienda> pedidos) =>
        Pagados(pedidos, ServicioPagoPedidoTienda.MercadoPago).Sum(p => p.Gestion);

    /// <summary>Transferencia confirmada y no devuelta: la plata no pasó por la plataforma y el local debe la gestión.</summary>
    public static decimal Pendiente(IEnumerable<PedidoTienda> pedidos) =>
        Pagados(pedidos, ServicioPagoPedidoTienda.Transferencia).Sum(p => p.Gestion);

    private static IEnumerable<PedidoTienda> Pagados(IEnumerable<PedidoTienda> pedidos, string medio) =>
        pedidos.Where(p => p.MedioPago == medio && p.EstadoPago == ServicioPagoPedidoTienda.Pagado);

    /// <summary>Transferencia confirmada, o efectivo (coordinar) ya entregado. Mercado Pago no: va por marketplace_fee.</summary>
    public static bool GeneraDeuda(PedidoTienda pedido) =>
        pedido.Gestion > 0 && (
            pedido.MedioPago == ServicioPagoPedidoTienda.Transferencia && pedido.EstadoPago == ServicioPagoPedidoTienda.Pagado
            || pedido.MedioPago == ServicioPagoPedidoTienda.Coordinar && pedido.Estado == ServicioPedidosTienda.Entregado);

    /// <summary>Null si no corresponde o si el pedido ya tiene fila. El importe es el congelado en el pedido.</summary>
    public static GestionTienda? Nacer(PedidoTienda pedido, DateTimeOffset? venceAbono, DateTimeOffset ahora, bool yaExiste)
    {
        if (yaExiste || !GeneraDeuda(pedido)) return null;
        return new GestionTienda
        {
            IdLocal = pedido.IdLocal,
            IdPedido = pedido.Id,
            Importe = pedido.Gestion,
            Medio = pedido.MedioPago,
            Estado = GestionTienda.Pendiente,
            Nacida = ahora,
            VencePeriodo = venceAbono ?? ahora
        };
    }

    /// <summary>Una devolución anula la deuda pendiente. Saldada queda saldada. Con un pago en curso, no se toca.</summary>
    public static void Anular(GestionTienda? deuda)
    {
        if (deuda == null || deuda.Estado is GestionTienda.Anulada or GestionTienda.Saldada) return;
        if (deuda.IdAbono != null)
            throw new ErrorNegocio("Hay un pago de la gestión en curso que incluye este pedido.");
        deuda.Estado = GestionTienda.Anulada;
    }

    public static DateTimeOffset Limite(GestionTienda deuda) => deuda.VencePeriodo.AddDays(DiasParaPagar);

    public static bool CierraTienda(IEnumerable<GestionTienda> deudas, DateTimeOffset ahora) =>
        deudas.Any(d => d.Estado == GestionTienda.Pendiente && ahora >= Limite(d));

    public static Abono NuevoCobro(int idLocal, IReadOnlyCollection<GestionTienda> pendientes, DateTimeOffset ahora)
    {
        if (pendientes.Count == 0)
            throw new ErrorNegocio("No hay gestión pendiente.");
        if (pendientes.Any(d => d.IdLocal != idLocal || d.Estado != GestionTienda.Pendiente || d.IdAbono != null))
            throw new ErrorNegocio("Hay un pago de la gestión en curso.");
        return new Abono
        {
            IdLocal = idLocal,
            Importe = pendientes.Sum(d => d.Importe),
            Estado = Abono.Pendiente,
            Origen = Abono.OrigenGestion,
            FechaCreacion = ahora
        };
    }

    public static void Reservar(IEnumerable<GestionTienda> deudas, int idAbono)
    {
        var lista = deudas.ToList();
        if (lista.Count == 0)
            throw new ErrorNegocio("No hay gestión pendiente.");
        foreach (var deuda in lista)
        {
            if (deuda.Estado != GestionTienda.Pendiente || deuda.IdAbono != null)
                throw new ErrorNegocio("Hay un pago de la gestión en curso.");
            deuda.IdAbono = idAbono;
        }
    }

    public static void Liberar(IEnumerable<GestionTienda> deudas)
    {
        foreach (var deuda in deudas)
        {
            if (deuda.Estado == GestionTienda.Pendiente)
                deuda.IdAbono = null;
        }
    }

    public static void RechazarPreferencia(Abono abono)
    {
        if (abono.Estado != Abono.Pendiente || abono.Origen != Abono.OrigenGestion)
            throw new ErrorNegocio("Ese pago de gestión ya no está pendiente.");
        abono.Estado = Abono.Rechazado;
    }

    /// <summary>Acredita el pago de la deuda. No mueve el vencimiento del abono.</summary>
    public static void AcreditarCobro(Abono abono, IEnumerable<GestionTienda> deudas, DateTimeOffset fecha, string idPagoMp)
    {
        if (abono.Origen != Abono.OrigenGestion)
            throw new ErrorNegocio("Ese pago no es de la gestión.");
        if (abono.Estado != Abono.Pendiente)
            throw new ErrorNegocio("El abono ya fue procesado.");
        var vinculadas = deudas.Where(d => d.IdAbono == abono.Id).ToList();
        if (vinculadas.Count == 0 || vinculadas.Any(d => d.Estado != GestionTienda.Pendiente))
            throw new ErrorNegocio("La gestión de ese pago ya no está pendiente.");
        if (vinculadas.Sum(d => d.Importe) != abono.Importe)
            throw new ErrorNegocio("El importe del pago no coincide con la gestión.");
        MarcarSaldadas(vinculadas, abono.Id, fecha);
        abono.Estado = Abono.Aprobado;
        abono.FechaAcreditacion = fecha;
        abono.IdPagoMp = idPagoMp;
    }

    public static Abono PrepararSaldoAdmin(int idLocal, IReadOnlyCollection<GestionTienda> pendientes, DateTimeOffset ahora)
    {
        if (pendientes.Count == 0 || pendientes.Any(d => d.IdLocal != idLocal || d.Estado != GestionTienda.Pendiente))
            throw new ErrorNegocio("No hay gestión pendiente para saldar.");
        if (pendientes.Any(d => d.IdAbono != null))
            throw new ErrorNegocio("Hay un pago de la gestión en curso.");
        return new Abono
        {
            IdLocal = idLocal,
            Importe = pendientes.Sum(d => d.Importe),
            Estado = Abono.Aprobado,
            Origen = Abono.OrigenGestionAdmin,
            FechaCreacion = ahora,
            FechaAcreditacion = ahora
        };
    }

    public static void MarcarSaldadas(IEnumerable<GestionTienda> deudas, int idAbono, DateTimeOffset ahora)
    {
        foreach (var deuda in deudas)
        {
            deuda.IdAbono = idAbono;
            deuda.Estado = GestionTienda.Saldada;
            deuda.FechaSaldo = ahora;
        }
    }

    public static void AnularSaldoAdmin(Abono abono, IEnumerable<GestionTienda> deudas)
    {
        if (abono.Origen != Abono.OrigenGestionAdmin || abono.Estado != Abono.Aprobado)
            throw new ErrorNegocio("No hay una gestión saldada por el admin para anular.");
        var propias = deudas.Where(d => d.IdAbono == abono.Id).ToList();
        if (propias.Count == 0)
            throw new ErrorNegocio("Ese saldo no tiene gestiones.");
        foreach (var deuda in propias)
        {
            deuda.Estado = GestionTienda.Pendiente;
            deuda.FechaSaldo = null;
            deuda.IdAbono = null;
        }
        abono.Estado = Abono.Anulado;
    }
}
