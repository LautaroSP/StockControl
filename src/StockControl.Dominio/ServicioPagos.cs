namespace StockControl.Dominio;

public static class ServicioPagos
{
    public static void Validar(decimal total, IReadOnlyCollection<PagoSolicitado> pagos)
    {
        if (pagos.Count == 0)
            throw new ErrorNegocio("Elegí al menos un medio de pago.");
        if (pagos.Any(p => p.Importe <= 0))
            throw new ErrorNegocio("Los importes de pago tienen que ser mayores a 0.");

        var suma = pagos.Sum(p => p.Importe);
        if (suma != total)
            throw new ErrorNegocio("Los medios de pago deben sumar exactamente el total.");
    }
}

public sealed record PagoSolicitado(int IdMetodoPago, decimal Importe);
