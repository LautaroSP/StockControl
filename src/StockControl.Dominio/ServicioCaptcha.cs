namespace StockControl.Dominio;

public static class ServicioCaptcha
{
    public static bool EstaActivo(string? siteKey, string? secreto) =>
        !string.IsNullOrWhiteSpace(siteKey) && !string.IsNullOrWhiteSpace(secreto);

    public static string? Falta(string? token) =>
        string.IsNullOrWhiteSpace(token) ? "Confirmá que no sos un robot." : null;
}
