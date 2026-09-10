namespace StockControl.Dominio;

public sealed class LineaTicket
{
    public required string Codigo { get; init; }
    public required string Nombre { get; init; }
    public decimal Cantidad { get; init; }
    public decimal Precio { get; init; }
    public decimal SubTotal { get; init; }
}

public sealed class Ticket
{
    public required string NombreLocal { get; init; }
    public DateTimeOffset Fecha { get; init; }
    public IReadOnlyList<LineaTicket> Lineas { get; init; } = [];
    public decimal Total { get; init; }
    public IReadOnlyList<string> Medios { get; init; } = [];
    public decimal DescuentoPorcentaje { get; init; }
    public bool AlCosto { get; init; }
}

public static class ServicioTicket
{
    public static string NormalizarFormato(string? formato)
    {
        var valor = (formato ?? "").Trim().ToUpperInvariant();
        if (valor is "" or "POS" or "POS80" or "POS-80") return "POS-80";
        if (valor is "POS-58" or "POS58") return "POS-58";
        if (valor == "A4") return "A4";
        throw new ErrorNegocio("El formato de ticket debe ser POS 58 mm, POS 80 mm o A4.");
    }

    public static string FormatoODefault(string? formato)
    {
        try { return NormalizarFormato(formato); }
        catch (ErrorNegocio) { return "POS-80"; }
    }

    public static Ticket Armar(
        string nombreLocal,
        DateTimeOffset fecha,
        IEnumerable<LineaVenta> lineas,
        decimal total,
        IEnumerable<string> medios,
        decimal descuentoPorcentaje,
        bool alCosto)
    {
        var lista = lineas.Select(l => new LineaTicket
        {
            Codigo = l.Codigo,
            Nombre = string.IsNullOrWhiteSpace(l.Nombre) ? l.Codigo : l.Nombre,
            Cantidad = l.Cantidad,
            Precio = l.Precio,
            SubTotal = l.SubTotal
        }).ToList();
        if (lista.Count == 0)
            throw new ErrorNegocio("El ticket no tiene ítems.");

        var mediosLista = medios
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Select(m => m.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new Ticket
        {
            NombreLocal = string.IsNullOrWhiteSpace(nombreLocal) ? "Local" : nombreLocal.Trim(),
            Fecha = fecha,
            Lineas = lista,
            Total = total,
            Medios = mediosLista,
            DescuentoPorcentaje = descuentoPorcentaje,
            AlCosto = alCosto
        };
    }

    public static IReadOnlyList<string> MediosDe(IEnumerable<PagoVenta> pagos, string metodoPago)
    {
        var dePagos = pagos
            .Where(p => !string.IsNullOrWhiteSpace(p.DescripcionMetodoPago))
            .Select(p => $"{p.DescripcionMetodoPago} {p.Importe:0.00}")
            .ToList();
        if (dePagos.Count > 0) return dePagos;
        if (!string.IsNullOrWhiteSpace(metodoPago)) return [metodoPago.Trim()];
        return [];
    }
}
