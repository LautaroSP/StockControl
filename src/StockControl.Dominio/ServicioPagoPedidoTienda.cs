namespace StockControl.Dominio;

/// <summary>Pago que se lee en la API de Mercado Pago con el token del local. Nunca el cuerpo del aviso.</summary>
public record PagoMercadoPagoTienda(string Id, string Estado, string Moneda, decimal Importe, string? Referencia, string? IdCobrador);

public enum ResultadoPagoTienda
{
    /// <summary>Todavía no está aprobado: no cambia nada.</summary>
    SinAprobar,
    /// <summary>Ese pago ya se había aplicado.</summary>
    Repetido,
    Acreditado,
    /// <summary>Aprobado para un pedido cancelado o ya pagado: hay que devolverlo.</summary>
    Devolver
}

/// <summary>Checkout Pro de la tienda con la cuenta del local. El pedido se paga solo con el pago verificado.</summary>
public static class ServicioPagoPedidoTienda
{
    public const string Coordinar = "coordinar";
    public const string MercadoPago = "mercadopago";
    public const string Transferencia = "transferencia";

    public const string SinPago = "sinpago";
    public const string Pendiente = "pendiente";
    public const string Pagado = "pagado";
    public const string Reembolsado = "reembolsado";

    public static readonly TimeSpan Plazo = TimeSpan.FromMinutes(30);

    private const string PrefijoReferencia = "pedido-tienda-";

    public static string NormalizarMedio(string? medio, bool aceptaMercadoPago, bool aceptaTransferencia)
    {
        var valor = string.IsNullOrWhiteSpace(medio) ? Coordinar : medio.Trim().ToLowerInvariant();
        if (valor is not (Coordinar or MercadoPago or Transferencia))
            throw new ErrorNegocio("El medio de pago no es válido.");
        if (valor == MercadoPago && !aceptaMercadoPago)
            throw new ErrorNegocio("Este local no recibe pagos con Mercado Pago.");
        if (valor == Transferencia && !aceptaTransferencia)
            throw new ErrorNegocio("Este local no recibe transferencias.");
        return valor;
    }

    public static string Referencia(int idPedido) => $"{PrefijoReferencia}{idPedido}";

    public static int? IdDeReferencia(string? referencia) =>
        referencia != null && referencia.StartsWith(PrefijoReferencia, StringComparison.Ordinal)
        && int.TryParse(referencia[PrefijoReferencia.Length..], out var id) ? id : null;

    /// <summary>Mercado Pago o transferencia sin acreditar: no se prepara ni se entrega.</summary>
    public static bool EsperaPago(PedidoTienda pedido) =>
        pedido.MedioPago is MercadoPago or Transferencia && pedido.EstadoPago == Pendiente;

    public static bool EsperaMercadoPago(PedidoTienda pedido) =>
        pedido.MedioPago == MercadoPago && pedido.EstadoPago == Pendiente;

    public static bool Vencido(PedidoTienda pedido, DateTimeOffset ahora) =>
        EsperaPago(pedido) && pedido.Estado == ServicioPedidosTienda.Nuevo && pedido.VencePago is { } vence && ahora >= vence;

    /// <summary>Esperando el pago no se avanza. La transferencia sin confirmar el local sí la puede cancelar.</summary>
    public static IReadOnlyList<string> Siguientes(PedidoTienda pedido)
    {
        var siguientes = ServicioPedidosTienda.Siguientes(pedido.Estado);
        if (!EsperaPago(pedido)) return siguientes;
        return pedido.MedioPago == Transferencia ? siguientes.Where(e => e == ServicioPedidosTienda.Cancelado).ToList() : [];
    }

    /// <summary>Sin pago en el plazo: se cancela y libera el stock.</summary>
    public static void Vencer(PedidoTienda pedido, DateTimeOffset ahora)
    {
        if (!Vencido(pedido, ahora))
            throw new ErrorNegocio("El pedido no venció.");
        ServicioPedidosTienda.Anular(pedido, ahora);
    }

    public static void ExigirSinPagoPendiente(PedidoTienda pedido)
    {
        if (EsperaPago(pedido))
            throw new ErrorNegocio(pedido.MedioPago == Transferencia
                ? "El pedido espera que confirmes la transferencia."
                : "El pedido espera el pago de Mercado Pago.");
    }

    /// <summary>El pago tiene que ser de este pedido, de la cuenta del local, en pesos y por el total.</summary>
    public static ResultadoPagoTienda Aplicar(PedidoTienda pedido, PagoMercadoPagoTienda pago, string idCuentaLocal, DateTimeOffset ahora)
    {
        if (pedido.MedioPago != MercadoPago || IdDeReferencia(pago.Referencia) != pedido.Id)
            throw new ErrorNegocio("El pago no es de este pedido.");
        if (string.IsNullOrEmpty(idCuentaLocal) || pago.IdCobrador != idCuentaLocal)
            throw new ErrorNegocio("El pago no entró en la cuenta del local.");
        if (pago.Estado != "approved")
            return ResultadoPagoTienda.SinAprobar;
        if (pago.Moneda != "ARS" || pago.Importe != pedido.Total)
            throw new ErrorNegocio("El pago no coincide con el total del pedido.");
        if (pedido.IdPagoMp == pago.Id)
            return ResultadoPagoTienda.Repetido;
        if (pedido.EstadoPago != Pendiente || pedido.Estado == ServicioPedidosTienda.Cancelado)
            return ResultadoPagoTienda.Devolver;

        pedido.EstadoPago = Pagado;
        pedido.IdPagoMp = pago.Id;
        pedido.Pagado = ahora;
        pedido.Actualizado = ahora;
        return ResultadoPagoTienda.Acreditado;
    }

    /// <summary>El pago que llegó tarde a un pedido cancelado ya se devolvió.</summary>
    public static void RegistrarDevolucion(PedidoTienda pedido, string idPago)
    {
        if (pedido.Estado != ServicioPedidosTienda.Cancelado || pedido.EstadoPago != Pendiente)
            return;
        pedido.EstadoPago = Reembolsado;
        pedido.IdPagoMp = idPago;
    }

    /// <summary>Solo el local cancela un pedido pagado. MP devuelve la gestión en proporción.</summary>
    public static void CancelarConReembolso(PedidoTienda pedido, DateTimeOffset ahora)
    {
        if (pedido.MedioPago != MercadoPago || pedido.EstadoPago != Pagado)
            throw new ErrorNegocio("El pedido no está pagado con Mercado Pago.");
        ServicioPedidosTienda.Anular(pedido, ahora);
        pedido.EstadoPago = Reembolsado;
    }
}
