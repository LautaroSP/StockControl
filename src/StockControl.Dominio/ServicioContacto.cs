namespace StockControl.Dominio;

public static class ServicioContacto
{
    public const int DiasHastaBorrar = 3;

    public static MensajeContacto Nuevo(string nombre, string correo, string mensaje, DateTimeOffset ahora)
    {
        var limpio = nombre.Trim();
        var mail = correo.Trim().ToLowerInvariant();
        var texto = mensaje.Trim();
        if (limpio.Length == 0 || mail.Length == 0 || texto.Length == 0)
            throw new ErrorNegocio("Completá nombre, correo y mensaje.");
        if (limpio.Length > 120 || mail.Length > 200 || texto.Length > 2000)
            throw new ErrorNegocio("El mensaje es demasiado largo.");
        if (!ServicioRegistro.EsMail(mail))
            throw new ErrorNegocio("El mail no es válido.");
        return new MensajeContacto
        {
            Nombre = limpio,
            Correo = mail,
            Mensaje = texto,
            Creado = ahora
        };
    }

    public static void MarcarLeido(MensajeContacto mensaje, DateTimeOffset ahora) =>
        mensaje.Leido ??= ahora;

    public static bool YaVencio(DateTimeOffset? leido, DateTimeOffset ahora) =>
        leido != null && ahora >= leido.Value.AddDays(DiasHastaBorrar);

    public static bool EstaActivo(string? valor) => valor is not ("0" or "false");
}
