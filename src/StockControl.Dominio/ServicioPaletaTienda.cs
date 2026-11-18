namespace StockControl.Dominio;

/// <summary>Fondo y botones de la tienda pública. Vacío es la paleta de StockControl.</summary>
public static class ServicioPaletaTienda
{
    public const int Largo = 7;
    public const string TintaOscura = "#1b241f";
    const double ContrasteMinimo = 4.5;

    /// <summary>Color del botón. El texto blanco tiene que leerse encima.</summary>
    public static string? Normalizar(string? color)
    {
        var hex = Hex(color);
        if (hex == null) return null;
        if (Contraste(hex, "#ffffff") < ContrasteMinimo)
            throw new ErrorNegocio("Ese color no se lee con el texto blanco del botón.");
        return hex;
    }

    /// <summary>Color de la página. El texto claro o el oscuro tiene que leerse encima.</summary>
    public static string? NormalizarFondo(string? color)
    {
        var hex = Hex(color);
        if (hex == null) return null;
        if (Contraste(hex, "#ffffff") < ContrasteMinimo && Contraste(hex, TintaOscura) < ContrasteMinimo)
            throw new ErrorNegocio("Ese fondo no se lee ni con texto claro ni con texto oscuro.");
        return hex;
    }

    /// <summary>El fondo es oscuro: el título y lo que está sobre la página van en blanco.</summary>
    public static bool TextoClaro(string? fondo) =>
        fondo != null && Contraste(fondo, TintaOscura) < ContrasteMinimo;

    /// <summary>Fondo de la ficha. El texto claro o el oscuro tiene que leerse encima.</summary>
    public static string? NormalizarFicha(string? color)
    {
        var hex = Hex(color);
        if (hex == null) return null;
        if (Contraste(hex, "#ffffff") < ContrasteMinimo && Contraste(hex, TintaOscura) < ContrasteMinimo)
            throw new ErrorNegocio("Esa ficha no se lee ni con texto claro ni con texto oscuro.");
        return hex;
    }

    public static void Asignar(Local local, string? boton, string? fondo, string? ficha = null)
    {
        local.ColorMarca = Normalizar(boton);
        local.ColorFondo = NormalizarFondo(fondo);
        local.ColorFicha = NormalizarFicha(ficha);
    }

    static string? Hex(string? color)
    {
        var texto = (color ?? "").Trim();
        if (texto.Length == 0) return null;
        if (!EsHex(texto))
            throw new ErrorNegocio("El color es # y seis números hexadecimales.");
        return texto.ToLowerInvariant();
    }

    static bool EsHex(string texto)
    {
        if (texto.Length != Largo || texto[0] != '#') return false;
        for (var i = 1; i < Largo; i++)
        {
            var c = texto[i];
            if (c is not (>= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F')) return false;
        }
        return true;
    }

    static double Contraste(string a, string b)
    {
        var clara = Math.Max(Luminancia(a), Luminancia(b));
        var oscura = Math.Min(Luminancia(a), Luminancia(b));
        return (clara + 0.05) / (oscura + 0.05);
    }

    static double Luminancia(string hex) =>
        0.2126 * Lineal(Canal(hex, 1)) + 0.7152 * Lineal(Canal(hex, 3)) + 0.0722 * Lineal(Canal(hex, 5));

    static int Canal(string hex, int desde) => Convert.ToInt32(hex.Substring(desde, 2), 16);

    static double Lineal(int canal)
    {
        var c = canal / 255d;
        return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
    }
}
