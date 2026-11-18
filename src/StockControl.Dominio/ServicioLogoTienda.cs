namespace StockControl.Dominio;

/// <summary>Logo de la tienda. Entra en 200 × 200 sin deformarse ni agrandarse.</summary>
public static class ServicioLogoTienda
{
    public const int Lado = 200;

    public static (int Ancho, int Alto) Dimensiones(int ancho, int alto) =>
        ServicioImagenesTienda.Dimensiones(ancho, alto, Lado, Lado);

    public static void Poner(Local local, string archivo)
    {
        if (string.IsNullOrWhiteSpace(archivo))
            throw new ErrorNegocio("Falta el archivo del logo.");
        local.LogoTienda = archivo;
    }

    public static void Quitar(Local local) => local.LogoTienda = null;
}
