namespace StockControl.Dominio;

public static class ServicioConsultaCajas
{
    public static IReadOnlyList<Caja> FilasTotal(IEnumerable<Caja> filas) =>
        filas.Where(c => c.MetodoPago == "Total").ToList();

    public static HashSet<int> NrosConMedio(IEnumerable<Caja> filas, string medio)
    {
        if (string.IsNullOrWhiteSpace(medio))
            return filas.Select(c => c.NroCaja).ToHashSet();
        return filas
            .Where(c => !string.Equals(c.MetodoPago, "Total", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(c.MetodoPago, medio, StringComparison.OrdinalIgnoreCase))
            .Select(c => c.NroCaja)
            .ToHashSet();
    }

    public static IReadOnlyList<Caja> FiltrarTotales(
        IEnumerable<Caja> todasLasFilas,
        string? quien,
        string? medio)
    {
        var lista = todasLasFilas.ToList();
        var totales = FilasTotal(lista);
        if (!string.IsNullOrWhiteSpace(medio))
        {
            var nros = NrosConMedio(lista, medio);
            totales = totales.Where(c => nros.Contains(c.NroCaja)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(quien))
        {
            totales = totales
                .Where(c => c.NombreCierre.Contains(quien, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        return totales.OrderByDescending(c => c.NroCaja).ToList();
    }

    public static (DateOnly desde, DateOnly hasta) MesActual(DateOnly hoy) =>
        (new DateOnly(hoy.Year, hoy.Month, 1), hoy);
}
