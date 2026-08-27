namespace StockControl.Dominio;

public class ResultadoCierre
{
    public int IdCierre { get; init; }
    public int NroCaja { get; init; }
    public List<Caja> Filas { get; init; } = new();
}

public static class ServicioCierre
{
    public static ResultadoCierre Cerrar(
        IReadOnlyList<InformeVenta> ventasDelDia,
        int nroPuesto,
        int idCierre,
        int idLocal,
        int? idUsuarioCierre,
        string nombreCierre,
        DateTimeOffset fecha,
        string tipoDesglose,
        IReadOnlyDictionary<int, string>? nombresUsuarios = null)
    {
        var desglose = string.Equals(tipoDesglose, TipoDesgloseCaja.Usuario, StringComparison.OrdinalIgnoreCase)
            ? TipoDesgloseCaja.Usuario
            : TipoDesgloseCaja.Medio;

        var abiertas = ventasDelDia
            .Where(v => v.IdCierre == null && v.NroCaja == nroPuesto)
            .ToList();
        if (abiertas.Count == 0)
            throw new ErrorNegocio("No hay ventas para cerrar en ese día.");

        List<Caja> filas;
        if (desglose == TipoDesgloseCaja.Usuario)
        {
            filas = abiertas
                .GroupBy(v => v.IdUsuario ?? 0)
                .OrderBy(g => NombreUsuario(g.Key, nombresUsuarios), StringComparer.OrdinalIgnoreCase)
                .Select(g => NuevaFila(
                    idLocal,
                    nroPuesto,
                    idCierre,
                    fecha,
                    NombreUsuario(g.Key, nombresUsuarios),
                    g.Sum(v => v.Total),
                    g.Count(),
                    idUsuarioCierre,
                    nombreCierre,
                    desglose))
                .ToList();
        }
        else
        {
            filas = abiertas
                .GroupBy(v => v.MetodoPago)
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .Select(g => NuevaFila(
                    idLocal,
                    nroPuesto,
                    idCierre,
                    fecha,
                    g.Key,
                    g.Sum(v => v.Total),
                    g.Count(),
                    idUsuarioCierre,
                    nombreCierre,
                    desglose))
                .ToList();
        }

        filas.Add(NuevaFila(
            idLocal,
            nroPuesto,
            idCierre,
            fecha,
            "Total",
            abiertas.Sum(v => v.Total),
            abiertas.Count,
            idUsuarioCierre,
            nombreCierre,
            desglose));

        foreach (var v in abiertas)
            v.IdCierre = idCierre;

        return new ResultadoCierre { IdCierre = idCierre, NroCaja = nroPuesto, Filas = filas };
    }

    private static string NombreUsuario(int idUsuario, IReadOnlyDictionary<int, string>? nombres)
    {
        if (idUsuario == 0)
            return "Sin usuario";
        if (nombres != null && nombres.TryGetValue(idUsuario, out var n) && !string.IsNullOrWhiteSpace(n))
            return n;
        return $"Usuario {idUsuario}";
    }

    private static Caja NuevaFila(
        int idLocal,
        int nroCaja,
        int idCierre,
        DateTimeOffset fecha,
        string medio,
        decimal total,
        int cantidad,
        int? idUsuario,
        string nombre,
        string tipoDesglose) =>
        new()
        {
            IdLocal = idLocal,
            NroCaja = nroCaja,
            IdCierre = idCierre,
            Fecha = fecha,
            MetodoPago = medio,
            Total = total,
            CantidadVentas = cantidad,
            IdUsuarioCierre = idUsuario,
            NombreCierre = nombre,
            TipoDesglose = tipoDesglose
        };
}
