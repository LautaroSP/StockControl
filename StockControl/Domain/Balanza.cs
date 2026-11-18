using System.Globalization;
using System.Text.RegularExpressions;

namespace StockControl.Domain
{
    public static class Balanza
    {
        private static readonly Regex PrimerNumero = new(@"-?\d+[.,]?\d*", RegexOptions.Compiled);

        public static bool PesoValido(decimal kilos) => kilos > 0m && kilos <= 100m;

        public static decimal Importe(decimal kilos, decimal precioPorKilo) =>
            Math.Round(kilos * precioPorKilo, 2, MidpointRounding.AwayFromZero);

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
    }
}
