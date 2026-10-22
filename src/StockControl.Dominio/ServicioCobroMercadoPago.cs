namespace StockControl.Dominio;

public static class ServicioCobroMercadoPago
{
    public static string ModoDe(IReadOnlyCollection<PagoConIntegracion> pagos, decimal total)
    {
        ServicioPagos.Validar(total, pagos.Select(p => new PagoSolicitado(p.IdMetodoPago, p.Importe)).ToList());
        var modos = pagos.Select(p => p.Integracion).Where(IntegracionPago.EsMercadoPago).Distinct().ToList();
        if (modos.Count == 0)
            throw new ErrorNegocio("Elegí un medio de Mercado Pago.");
        if (modos.Count > 1)
            throw new ErrorNegocio("Solo un medio de Mercado Pago por cobro.");
        return modos[0];
    }

    public const decimal MinimoQr = 15m;

    public static decimal ImporteDeLaOrden(IReadOnlyCollection<PagoConIntegracion> pagos) =>
        pagos.Where(p => IntegracionPago.EsMercadoPago(p.Integracion)).Sum(p => p.Importe);

    public static void ValidarImporteQr(string modo, decimal importe)
    {
        if (modo is IntegracionPago.QrImpreso or IntegracionPago.QrPantalla && importe < MinimoQr)
            throw new ErrorNegocio("El QR de Mercado Pago pide al menos $ 15.");
    }

    public static void ValidarApertura(bool vinculada, string modo, bool tieneQr, bool tieneLector, bool hayAbierto)
    {
        if (!vinculada)
            throw new ErrorNegocio("Este local no tiene Mercado Pago conectado.");
        if (!IntegracionPago.EsMercadoPago(modo))
            throw new ErrorNegocio("Ese medio no espera a Mercado Pago.");
        if (modo is IntegracionPago.QrImpreso or IntegracionPago.QrPantalla && !tieneQr)
            throw new ErrorNegocio("Prepará el QR de este puesto en Configuración.");
        if (modo == IntegracionPago.Point && !tieneLector)
            throw new ErrorNegocio("Elegí el lector de este puesto en Configuración.");
        if (modo == IntegracionPago.QrImpreso && hayAbierto)
            throw new ErrorNegocio("Este puesto ya tiene un cobro abierto en el QR impreso.");
    }

    public static string ModoOrden(string modo) => modo switch
    {
        IntegracionPago.QrImpreso => "static",
        IntegracionPago.QrPantalla => "dynamic",
        IntegracionPago.Point => "point",
        _ => throw new ErrorNegocio("Ese medio no espera a Mercado Pago.")
    };

    public static string EstadoDesdeOrden(string? estado) => estado switch
    {
        "processed" => EstadoCobroMercadoPago.Pagado,
        "canceled" => EstadoCobroMercadoPago.Cancelado,
        "expired" => EstadoCobroMercadoPago.Vencido,
        _ => EstadoCobroMercadoPago.Esperando
    };

    /// <summary>True la primera vez. Un aviso repetido no vuelve a confirmar la venta.</summary>
    public static bool IntentarMarcarPagado(CobroMercadoPago cobro)
    {
        if (cobro.Estado == EstadoCobroMercadoPago.Pagado)
            return false;
        if (cobro.Estado != EstadoCobroMercadoPago.Esperando)
            throw new ErrorNegocio("El cobro ya no está esperando.");
        cobro.Estado = EstadoCobroMercadoPago.Pagado;
        return true;
    }

    public static void Cancelar(CobroMercadoPago cobro)
    {
        if (cobro.Estado == EstadoCobroMercadoPago.Pagado)
            throw new ErrorNegocio("El cobro ya está pagado.");
        if (cobro.Estado != EstadoCobroMercadoPago.Esperando)
            return;
        cobro.Estado = EstadoCobroMercadoPago.Cancelado;
    }

    public static void Vencer(CobroMercadoPago cobro)
    {
        if (cobro.Estado != EstadoCobroMercadoPago.Esperando)
            return;
        cobro.Estado = EstadoCobroMercadoPago.Vencido;
    }
}

public sealed record PagoConIntegracion(int IdMetodoPago, string Integracion, decimal Importe);

public class PedidoGuardado
{
    public List<ItemCarrito> Items { get; set; } = new();
    public List<PagoSolicitado> Pagos { get; set; } = new();
    public int DescuentoPorcentaje { get; set; }
    public bool CobrarAlCosto { get; set; }
}
