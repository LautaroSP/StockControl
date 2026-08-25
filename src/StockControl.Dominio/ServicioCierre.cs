namespace StockControl.Dominio;

public class ResultadoCierre
{
    public int NroCaja { get; init; }
    public List<Caja> Filas { get; init; } = new();
}

public static class ServicioCierre
{
    public static ResultadoCierre Cerrar(
        IReadOnlyList<InformeVenta> ventasDelDia,
        int nroCaja,
        int idLocal,
        int? idUsuarioCierre,
        string nombreCierre,
        DateTimeOffset fecha)
    {
        var abiertas = ventasDelDia.Where(v => v.NroCaja == null).ToList();
        if (abiertas.Count == 0)
            throw new ErrorNegocio("No hay ventas para cerrar en ese día.");

        var filas = abiertas
            .GroupBy(v => v.MetodoPago)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .Select(g => NuevaFila(idLocal, nroCaja, fecha, g.Key, g.Sum(v => v.Total), g.Count(), idUsuarioCierre, nombreCierre))
            .ToList();

        filas.Add(NuevaFila(
            idLocal,
            nroCaja,
            fecha,
            "Total",
            abiertas.Sum(v => v.Total),
            abiertas.Count,
            idUsuarioCierre,
            nombreCierre));

        foreach (var v in abiertas)
            v.NroCaja = nroCaja;

        return new ResultadoCierre { NroCaja = nroCaja, Filas = filas };
    }

    private static Caja NuevaFila(
        int idLocal,
        int nroCaja,
        DateTimeOffset fecha,
        string medio,
        decimal total,
        int cantidad,
        int? idUsuario,
        string nombre) =>
        new()
        {
            IdLocal = idLocal,
            NroCaja = nroCaja,
            Fecha = fecha,
            MetodoPago = medio,
            Total = total,
            CantidadVentas = cantidad,
            IdUsuarioCierre = idUsuario,
            NombreCierre = nombre
        };
}
