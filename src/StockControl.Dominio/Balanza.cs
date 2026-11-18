using System.Globalization;
using System.Text.RegularExpressions;

namespace StockControl.Dominio;

public static class Balanza
{
    private static readonly Regex PrimerNumero = new(@"-?\d+[.,]?\d*", RegexOptions.Compiled);

    public static bool EnListado(decimal precioPorKilo) => precioPorKilo > 0m;

    public static bool PesoValido(decimal kilos) => kilos > 0m && kilos <= 100m;

    public static decimal Importe(decimal kilos, decimal precioPorKilo) =>
        Math.Round(kilos * precioPorKilo, 2, MidpointRounding.AwayFromZero);

    public static decimal TotalCarrito(IEnumerable<(decimal Kilos, decimal PrecioPorKilo)> pesadas) =>
        Math.Round(pesadas.Sum(p => Importe(p.Kilos, p.PrecioPorKilo)), 2, MidpointRounding.AwayFromZero);

    public static bool TryParsePeso(string? texto, out decimal kilos)
    {
        kilos = 0m;
        if (string.IsNullOrWhiteSpace(texto))
            return false;

        var match = PrimerNumero.Match(texto);
        if (!match.Success)
            return false;

        var numero = match.Value.Replace(',', '.');
        return decimal.TryParse(numero, NumberStyles.Number, CultureInfo.InvariantCulture, out kilos);
    }

    /// <summary>Si el texto es un código de producto, no se toma como peso.</summary>
    public static bool IntentarPeso(string? texto, bool esCodigoProducto, out decimal kilos)
    {
        kilos = 0m;
        if (esCodigoProducto)
            return false;
        return TryParsePeso(texto, out kilos) && PesoValido(kilos);
    }
}
