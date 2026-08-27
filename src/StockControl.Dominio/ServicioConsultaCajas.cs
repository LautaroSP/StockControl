namespace StockControl.Dominio;

public static class ServicioConsultaCajas
{
    public static IReadOnlyList<Caja> FilasTotal(IEnumerable<Caja> filas) =>
        filas.Where(c => c.MetodoPago == "Total").ToList();

    public static HashSet<int> IdsCierreConMedio(IEnumerable<Caja> filas, string medio)
    {
        if (string.IsNullOrWhiteSpace(medio))
            return filas.Select(c => c.IdCierre).ToHashSet();
        return filas
            .Where(c => !string.Equals(c.MetodoPago, "Total", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(c.MetodoPago, medio, StringComparison.OrdinalIgnoreCase))
            .Select(c => c.IdCierre)
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
            var ids = IdsCierreConMedio(lista, medio);
            totales = totales.Where(c => ids.Contains(c.IdCierre)).ToList();
        }
        if (!string.IsNullOrWhiteSpace(quien))
        {
            totales = totales
                .Where(c => c.NombreCierre.Contains(quien, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        return totales.OrderByDescending(c => c.IdCierre).ToList();
    }

    public static (DateOnly desde, DateOnly hasta) MesActual(DateOnly hoy) =>
        (new DateOnly(hoy.Year, hoy.Month, 1), hoy);
}
