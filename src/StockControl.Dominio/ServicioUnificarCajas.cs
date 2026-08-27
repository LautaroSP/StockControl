namespace StockControl.Dominio;

public class GrupoUnificable
{
    public int NroCaja { get; init; }
    public string NombreCierre { get; init; } = "";
    public DateOnly Dia { get; init; }
    public List<int> IdsCierre { get; init; } = new();
}

public class ResultadoUnificar
{
    public int IdCierre { get; init; }
    public List<Caja> Filas { get; init; } = new();
    public IReadOnlyList<int> IdsEliminados { get; init; } = Array.Empty<int>();
}

public static class ServicioUnificarCajas
{
    public static IReadOnlyList<GrupoUnificable> GruposUnificables(IEnumerable<Caja> filasTotal)
    {
        return filasTotal
            .Where(c => c.MetodoPago == "Total")
            .GroupBy(c => new
            {
                c.NroCaja,
                Nombre = c.NombreCierre.Trim().ToLowerInvariant(),
                Dia = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(c.Fecha, DiaArgentina.Zona).DateTime)
            })
            .Where(g => g.Select(x => x.IdCierre).Distinct().Count() >= 2)
            .Select(g => new GrupoUnificable
            {
                NroCaja = g.Key.NroCaja,
                NombreCierre = g.First().NombreCierre,
                Dia = g.Key.Dia,
                IdsCierre = g.Select(x => x.IdCierre).Distinct().OrderBy(id => id).ToList()
            })
            .OrderByDescending(g => g.Dia)
            .ThenBy(g => g.NroCaja)
            .ToList();
    }

    public static ResultadoUnificar Unificar(IReadOnlyList<Caja> todasLasFilasDeLosCierres, IReadOnlyList<int> idsCierre)
    {
        var ids = idsCierre.Distinct().OrderBy(x => x).ToList();
        if (ids.Count < 2)
            throw new ErrorNegocio("Elegí al menos dos cierres para unificar.");

        var filas = todasLasFilasDeLosCierres.Where(c => ids.Contains(c.IdCierre)).ToList();
        if (filas.Count == 0)
            throw new ErrorNegocio("No se encontraron esos cierres.");

        var totales = filas.Where(c => c.MetodoPago == "Total").ToList();
        if (totales.Count < 2)
            throw new ErrorNegocio("Hacen falta al menos dos cierres con fila Total.");

        var puesto = totales[0].NroCaja;
        var quien = totales[0].NombreCierre.Trim();
        var dia = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(totales[0].Fecha, DiaArgentina.Zona).DateTime);
        var desglose = totales[0].TipoDesglose;
        var idLocal = totales[0].IdLocal;
        var idUsuario = totales[0].IdUsuarioCierre;

        foreach (var t in totales)
        {
            if (t.NroCaja != puesto)
                throw new ErrorNegocio("Solo se unifican cierres del mismo puesto.");
            if (!string.Equals(t.NombreCierre.Trim(), quien, StringComparison.OrdinalIgnoreCase))
                throw new ErrorNegocio("Solo se unifican cierres de la misma persona.");
            var d = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(t.Fecha, DiaArgentina.Zona).DateTime);
            if (d != dia)
                throw new ErrorNegocio("Solo se unifican cierres del mismo día.");
            if (!string.Equals(t.TipoDesglose, desglose, StringComparison.OrdinalIgnoreCase))
                throw new ErrorNegocio("Solo se unifican cierres con el mismo tipo de desglose.");
        }

        var idDestino = ids[0];
        var fecha = totales.Min(t => t.Fecha);
        var parciales = filas.Where(c => c.MetodoPago != "Total").ToList();
        var agrupadas = parciales
            .GroupBy(c => c.MetodoPago, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
            .Select(g => new Caja
            {
                IdLocal = idLocal,
                NroCaja = puesto,
                IdCierre = idDestino,
                Fecha = fecha,
                MetodoPago = g.First().MetodoPago,
                Total = g.Sum(x => x.Total),
                CantidadVentas = g.Sum(x => x.CantidadVentas),
                IdUsuarioCierre = idUsuario,
                NombreCierre = totales[0].NombreCierre,
                TipoDesglose = desglose
            })
            .ToList();

        agrupadas.Add(new Caja
        {
            IdLocal = idLocal,
            NroCaja = puesto,
            IdCierre = idDestino,
            Fecha = fecha,
            MetodoPago = "Total",
            Total = agrupadas.Sum(x => x.Total),
            CantidadVentas = agrupadas.Sum(x => x.CantidadVentas),
            IdUsuarioCierre = idUsuario,
            NombreCierre = totales[0].NombreCierre,
            TipoDesglose = desglose
        });

        return new ResultadoUnificar
        {
            IdCierre = idDestino,
            Filas = agrupadas,
            IdsEliminados = ids.Skip(1).ToList()
        };
    }
}
