namespace StockControl.Dominio;

/// <summary>CUIT argentino: 11 dígitos con verificador (módulo 11).</summary>
public static class Cuit
{
    private static readonly int[] Pesos = [5, 4, 3, 2, 7, 6, 5, 4, 3, 2];

    public static string Normalizar(string? texto) =>
        new((texto ?? "").Where(char.IsDigit).ToArray());

    public static bool EsValido(string? texto)
    {
        var d = Normalizar(texto);
        if (d.Length != 11) return false;
        var suma = 0;
        for (var i = 0; i < 10; i++)
            suma += (d[i] - '0') * Pesos[i];
        var resto = 11 - suma % 11;
        var verificador = resto == 11 ? 0 : resto;
        return verificador != 10 && verificador == d[10] - '0';
    }
}
