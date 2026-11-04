namespace StockControl.Dominio;

public static class ServicioLogin
{
    public const int MaximoFallos = 5;
    public static readonly TimeSpan Freno = TimeSpan.FromMinutes(15);

    public static bool EstaFrenado(Usuario usuario, DateTimeOffset ahora) =>
        usuario.BloqueadoHasta is { } hasta && hasta > ahora;

    public static void RegistrarFallo(Usuario usuario, DateTimeOffset ahora)
    {
        if (EstaFrenado(usuario, ahora))
            return;
        usuario.IntentosFallidos++;
        if (usuario.IntentosFallidos < MaximoFallos)
            return;
        usuario.BloqueadoHasta = ahora.Add(Freno);
        usuario.IntentosFallidos = 0;
    }

    public static void RegistrarAcierto(Usuario usuario)
    {
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
    }
}
