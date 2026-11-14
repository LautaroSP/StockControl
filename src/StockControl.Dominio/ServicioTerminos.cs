namespace StockControl.Dominio;

public static class ServicioTerminos
{
    public const string Version = "1";

    public static void Exigir(bool acepta)
    {
        if (!acepta)
            throw new ErrorNegocio("Tenés que aceptar los términos y la política de privacidad.");
    }

    public static void Marcar(Usuario usuario, DateTimeOffset ahora)
    {
        usuario.VersionTerminos = Version;
        usuario.AceptoTerminosEn = ahora;
    }

    public static bool DebeAceptar(string? rol) => rol is Roles.Dueno or Roles.Socio;

    public static bool Vigente(Usuario usuario) => usuario.VersionTerminos == Version;
}
