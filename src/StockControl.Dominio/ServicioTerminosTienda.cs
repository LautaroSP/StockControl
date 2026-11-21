namespace StockControl.Dominio;

/// <summary>Términos de la tienda online: gestión por pedido y deuda con el prestador.</summary>
public static class ServicioTerminosTienda
{
    public const string Version = "1";

    public static void Exigir(bool acepta)
    {
        if (!acepta)
            throw new ErrorNegocio("Tenés que aceptar los términos de la tienda online.");
    }

    public static void Marcar(Local local, DateTimeOffset ahora)
    {
        local.VersionTerminosTienda = Version;
        local.AceptoTerminosTiendaEn = ahora;
    }

    public static bool Vigente(Local local) => local.VersionTerminosTienda == Version;

    /// <summary>Para publicar (activa) hace falta la versión vigente. Pausar o apagar no exige.</summary>
    public static void ExigirAlPublicar(Local local, string? estado, bool acepta, DateTimeOffset ahora)
    {
        var valor = (estado ?? "").Trim().ToLowerInvariant();
        if (valor != ServicioTienda.Activa) return;
        if (Vigente(local)) return;
        Exigir(acepta);
        Marcar(local, ahora);
    }
}
