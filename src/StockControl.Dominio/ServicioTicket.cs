namespace StockControl.Dominio;

public sealed record AhorroTicket(string Descripcion, decimal Importe);

public sealed record MedioTicket(string Descripcion, decimal Importe);

public sealed class LineaTicket
{
    public required string Nombre { get; init; }
    public decimal Cantidad { get; init; }
    public decimal Precio { get; init; }
    public decimal SubTotal { get; init; }
    public AhorroTicket? Ahorro { get; init; }
    /// <summary>Solo en ventas anteriores a los ahorros: la oferta que tenía la línea.</summary>
    public string Nota { get; init; } = "";
}

public sealed class Ticket
{
    public DateTimeOffset Fecha { get; init; }
    public int? NroCaja { get; init; }
    public IReadOnlyList<LineaTicket> Lineas { get; init; } = [];
    public IReadOnlyList<AhorroTicket> Ahorros { get; init; } = [];
    public decimal Total { get; init; }
    public IReadOnlyList<MedioTicket> Medios { get; init; } = [];
    public decimal Items { get; init; }
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

    public static Ticket DeVenta(InformeVenta venta) => Armar(
        venta.Detalles,
        venta.Total,
        MediosDe(venta.Pagos, venta.MetodoPago, venta.Total),
        venta.Descuento,
        venta.Fecha,
        venta.NroCaja);

    public static Ticket DeCuenta(CuentaPendiente cuenta, string cliente) => Armar(
        cuenta.Lineas,
        cuenta.Importe,
        [new MedioTicket($"A cuenta de {cliente}", cuenta.Importe)],
        0,
        cuenta.Fecha,
        cuenta.NroCaja);

    public static Ticket Armar(
        IEnumerable<ILineaVendida> lineas,
        decimal total,
        IReadOnlyList<MedioTicket> medios,
        decimal descuentoPorcentaje,
        DateTimeOffset fecha,
        int? nroCaja)
    {
        var lista = lineas.ToList();
        if (lista.Count == 0)
            throw new ErrorNegocio("El ticket no tiene ítems.");

        return new Ticket
        {
            Fecha = fecha,
            NroCaja = nroCaja,
            Lineas = lista.Select(ArmarLinea).ToList(),
            Ahorros = [.. AhorrosDeCombos(lista), .. AhorroDeDescuento(lista, descuentoPorcentaje)],
            Total = total,
            Medios = medios,
            Items = lista.Sum(l => l.Cantidad)
        };
    }

    public static IReadOnlyList<MedioTicket> MediosDe(IEnumerable<PagoVenta> pagos, string metodoPago, decimal total)
    {
        var dePagos = pagos
            .Where(p => !string.IsNullOrWhiteSpace(p.DescripcionMetodoPago))
            .Select(p => new MedioTicket(p.DescripcionMetodoPago.Trim(), p.Importe))
            .ToList();
        if (dePagos.Count > 0) return dePagos;
        if (!string.IsNullOrWhiteSpace(metodoPago)) return [new MedioTicket(metodoPago.Trim(), total)];
        return [];
    }

    private static LineaTicket ArmarLinea(ILineaVendida l)
    {
        var nombre = string.IsNullOrWhiteSpace(l.Nombre) ? l.Codigo : l.Nombre;
        if (l.PrecioBase is not decimal precioBase)
            return new LineaTicket
            {
                Nombre = nombre,
                Cantidad = l.Cantidad,
                Precio = l.Precio,
                SubTotal = l.SubTotal,
                Nota = l.DescripcionOferta
            };

        return new LineaTicket
        {
            Nombre = nombre,
            Cantidad = l.Cantidad,
            Precio = precioBase,
            SubTotal = Math.Round(precioBase * l.Cantidad, 2, MidpointRounding.AwayFromZero),
            Ahorro = l.AhorroOferta != 0 ? new AhorroTicket($"Oferta {l.DescripcionOferta}".Trim(), l.AhorroOferta) : null
        };
    }

    private static IEnumerable<AhorroTicket> AhorrosDeCombos(IEnumerable<ILineaVendida> lineas) =>
        lineas
            .Where(l => l.PrecioBase != null && l.AhorroCombo != 0)
            .GroupBy(l => l.NombreCombo)
            .Select(g => new AhorroTicket($"Combo {g.Key}".Trim(), g.Sum(l => l.AhorroCombo)));

    private static IEnumerable<AhorroTicket> AhorroDeDescuento(IEnumerable<ILineaVendida> lineas, decimal porcentaje)
    {
        var importe = lineas.Where(l => l.PrecioBase != null).Sum(l => l.AhorroDescuento);
        if (importe == 0) yield break;
        yield return new AhorroTicket(porcentaje > 0 ? $"Descuento {porcentaje:0.##} %" : "Descuento", importe);
    }
}
