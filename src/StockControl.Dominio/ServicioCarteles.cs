namespace StockControl.Dominio;

public sealed class Cartel
{
    public required string Codigo { get; init; }
    public required string Nombre { get; init; }
    public decimal Precio { get; init; }
}

public static class ServicioCarteles
{
    public const string Grande = "grande";
    public const string Chico = "chico";
    public const int MaxBytesPlantilla = 1024 * 1024;

    public static string NormalizarTamano(string? tamano)
    {
        var valor = (tamano ?? "").Trim().ToLowerInvariant();
        if (valor is Grande or Chico) return valor;
        throw new ErrorNegocio("El tamaño del cartel debe ser grande o chico.");
    }

    public static int CartelesPorHoja(string tamano) =>
        NormalizarTamano(tamano) == Grande ? 6 : 30;

    public static IReadOnlyList<IReadOnlyList<Cartel>> ArmarHojas(IEnumerable<Producto> productos, string tamano)
    {
        var porHoja = CartelesPorHoja(tamano);
        return productos
            .Select(p => new Cartel { Codigo = p.Codigo, Nombre = p.Nombre, Precio = Math.Truncate(p.Precio) })
            .Chunk(porHoja)
            .Select(h => (IReadOnlyList<Cartel>)h)
            .ToList();
    }

    public static string ValidarPlantilla(byte[]? datos)
    {
        if (datos == null || datos.Length == 0)
            throw new ErrorNegocio("La imagen está vacía.");
        if (datos.Length > MaxBytesPlantilla)
            throw new ErrorNegocio("La imagen no puede superar 1 MB.");
        if (EsPng(datos)) return "image/png";
        if (EsJpeg(datos)) return "image/jpeg";
        throw new ErrorNegocio("La plantilla debe ser PNG o JPG.");
    }

    private static bool EsPng(byte[] d) =>
        d.Length >= 8 && d[0] == 0x89 && d[1] == 0x50 && d[2] == 0x4E && d[3] == 0x47
        && d[4] == 0x0D && d[5] == 0x0A && d[6] == 0x1A && d[7] == 0x0A;

    private static bool EsJpeg(byte[] d) =>
        d.Length >= 3 && d[0] == 0xFF && d[1] == 0xD8 && d[2] == 0xFF;
}
