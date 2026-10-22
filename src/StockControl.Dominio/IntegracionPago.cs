namespace StockControl.Dominio;

public static class IntegracionPago
{
    public const string Manual = "manual";
    public const string QrImpreso = "qr_impreso";
    public const string QrPantalla = "qr_pantalla";
    public const string Point = "point";

    public static bool EsMercadoPago(string? integracion) =>
        integracion is QrImpreso or QrPantalla or Point;

    public static string Normalizar(string? valor)
    {
        var texto = string.IsNullOrWhiteSpace(valor) ? Manual : valor.Trim().ToLowerInvariant();
        if (texto is not (Manual or QrImpreso or QrPantalla or Point))
            throw new ErrorNegocio("La integración del medio no es válida.");
        return texto;
    }
}
