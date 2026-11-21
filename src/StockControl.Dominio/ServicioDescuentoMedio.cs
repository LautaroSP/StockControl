namespace StockControl.Dominio;

public readonly record struct DescuentoMedioCalculado(int Porcentaje, decimal Importe, decimal Total);

/// <summary>
/// Porcentaje del total ya armado (ofertas, combos y descuento de caja).
/// Corre solo si un único medio paga la venta y no es un pago de cuenta.
/// </summary>
public static class ServicioDescuentoMedio
{
    public static int Exigir(int porcentaje)
    {
        if (porcentaje is < 0 or > 99)
            throw new ErrorNegocio("El descuento del medio va de 0 a 99 %.");
        return porcentaje;
    }

    public static bool EsPagoDeCuenta(IEnumerable<string> codigos) =>
        codigos.Any(c => c.StartsWith(ServicioCuentas.PrefijoCodigo, StringComparison.OrdinalIgnoreCase));

    public static DescuentoMedioCalculado Calcular(decimal total, IReadOnlyList<(int Id, int Porcentaje)> medios, bool esPagoDeCuenta)
    {
        var ids = medios.Select(m => m.Id).Distinct().ToList();
        if (esPagoDeCuenta || ids.Count != 1)
            return new DescuentoMedioCalculado(0, 0, total);
        var porcentaje = medios.First(m => m.Id == ids[0]).Porcentaje;
        if (porcentaje <= 0)
            return new DescuentoMedioCalculado(0, 0, total);
        var importe = Math.Round(total * porcentaje / 100m, 2, MidpointRounding.AwayFromZero);
        if (importe <= 0)
            return new DescuentoMedioCalculado(0, 0, total);
        var cobrado = total - importe;
        if (cobrado <= 0)
            throw new ErrorNegocio("El descuento del medio deja el total en cero.");
        return new DescuentoMedioCalculado(porcentaje, importe, cobrado);
    }

    public static void Aplicar(ResultadoCobro resultado, IReadOnlyList<(int Id, int Porcentaje)> medios)
    {
        var calculado = Calcular(resultado.Total, medios, EsPagoDeCuenta(resultado.Lineas.Select(l => l.Codigo)));
        resultado.Total = calculado.Total;
        resultado.DescuentoMedioPorcentaje = calculado.Porcentaje;
        resultado.DescuentoMedio = calculado.Importe;
    }

    /// <summary>El porcentaje quedó fijado al iniciar el cobro. No se vuelve a leer el medio.</summary>
    public static void AplicarGuardado(ResultadoCobro resultado, int porcentaje)
    {
        if (porcentaje <= 0 || EsPagoDeCuenta(resultado.Lineas.Select(l => l.Codigo)))
            return;
        var calculado = Calcular(resultado.Total, [(0, porcentaje)], false);
        resultado.Total = calculado.Total;
        resultado.DescuentoMedioPorcentaje = calculado.Porcentaje;
        resultado.DescuentoMedio = calculado.Importe;
    }
}
