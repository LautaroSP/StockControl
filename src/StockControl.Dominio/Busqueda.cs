namespace StockControl.Dominio;

public static class Busqueda
{
    public const int Maximo = 80;

    /// <summary>Patrón de <c>ILIKE</c>. Vacío no filtra. Escapa <c>\</c>, <c>%</c> y <c>_</c>.</summary>
    public static string? Patron(string? texto)
    {
        var recortado = (texto ?? "").Trim();
        if (recortado.Length == 0) return null;
        if (recortado.Length > Maximo) recortado = recortado[..Maximo];
        return "%" + Escapar(recortado) + "%";
    }

    static string Escapar(string texto) =>
        texto.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
}
