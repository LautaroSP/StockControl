using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace StockControl
{
    public static class TextoHelper
    {

        public static string LimpiarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return texto;

            // Normaliza (separa letras de acentos)
            string normalized = texto.Normalize(NormalizationForm.FormD);

            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                UnicodeCategory uc = Char.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            // Quita caracteres raros y deja ASCII visible
            string limpio = sb.ToString().Normalize(NormalizationForm.FormC);

            limpio = Regex.Replace(limpio, @"[^\x20-\x7E]", "");

            return limpio;
        }

    }
}
