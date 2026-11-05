namespace StockControl.Dominio;

/// <summary>QR que abre la tienda del comprador. En la hoja hay tres tamaños; en la POS uno solo.</summary>
public static class ServicioQrTienda
{
    public const string Chico = "chico";
    public const string Mediano = "mediano";
    public const string Grande = "grande";
    public const string Marca = "StockControl";

    /// <summary>Ancho del afiche en la impresora térmica. No sigue al tamaño de la hoja.</summary>
    public const int LadoPosMm = 50;

    public static string NormalizarTamano(string? tamano)
    {
        var valor = (tamano ?? "").Trim().ToLowerInvariant();
        if (valor is Chico or Mediano or Grande) return valor;
        throw new ErrorNegocio("El tamaño del QR debe ser chico, mediano o grande.");
    }

    /// <summary>Ancho del afiche en la hoja, en milímetros.</summary>
    public static int LadoMm(string? tamano) => NormalizarTamano(tamano) switch
    {
        Chico => 80,
        Mediano => 120,
        _ => 160
    };

    /// <summary>Ancho del PNG que se descarga, en píxeles.</summary>
    public static int LadoPx(string? tamano) => NormalizarTamano(tamano) switch
    {
        Chico => 800,
        Mediano => 1200,
        _ => 1600
    };

    /// <summary>Ancho del papel de la POS. Con ticket A4 también sale en 80 mm.</summary>
    public static int AnchoPapelPosMm(string? formatoTicket) =>
        ServicioTicket.FormatoODefault(formatoTicket) == "POS-58" ? 58 : 80;

    /// <summary>Ruta pública. El origen lo pone el navegador.</summary>
    public static string RutaComprador(string? alias)
    {
        var normalizado = ServicioTienda.NormalizarAlias(alias);
        if (normalizado == null)
            throw new ErrorNegocio("La tienda no tiene alias. Sin alias no hay QR.");
        return $"/tienda/{normalizado}";
    }
}
