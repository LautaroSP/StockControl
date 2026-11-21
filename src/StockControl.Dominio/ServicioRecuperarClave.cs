namespace StockControl.Dominio;

/// <summary>
/// Solo dueño y socio, que se registran con mail. El empleado se la resetea el dueño.
/// En la base queda el hash del token, nunca el link.
/// </summary>
public static class ServicioRecuperarClave
{
    public const int MinutosDeVigencia = 10;
    public const int MinutosEntrePedidos = 1;
    public const string Respuesta = "Si la cuenta existe, te mandamos un mail con el link. Vence en 10 minutos.";
    const string LinkInvalido = "El link no sirve. Pedí otro desde «¿Olvidaste tu contraseña?».";

    public static bool PuedeRecuperar(Usuario? usuario) =>
        usuario is { Activo: true, EmailConfirmado: true }
        && usuario.Rol is Roles.Dueno or Roles.Socio
        && !string.IsNullOrWhiteSpace(usuario.Email);

    /// <summary>Devuelve el token para el mail, o null si no corresponde mandar otro.</summary>
    public static string? Iniciar(Usuario? usuario, DateTimeOffset ahora)
    {
        if (!PuedeRecuperar(usuario))
            return null;
        if (usuario!.VenceRecuperacion is { } vence && ahora < vence.AddMinutes(MinutosEntrePedidos - MinutosDeVigencia))
            return null;
        var token = ServicioConfirmacionCorreo.NuevoToken();
        usuario.TokenRecuperacion = ServicioConfirmacionCorreo.Hash(token);
        usuario.VenceRecuperacion = ahora.AddMinutes(MinutosDeVigencia);
        return token;
    }

    public static string? Rechazo(string? token, Usuario? usuario, DateTimeOffset ahora)
    {
        if (string.IsNullOrWhiteSpace(token) || !PuedeRecuperar(usuario)
            || string.IsNullOrWhiteSpace(usuario!.TokenRecuperacion) || usuario.VenceRecuperacion == null
            || !string.Equals(ServicioConfirmacionCorreo.Hash(token), usuario.TokenRecuperacion, StringComparison.Ordinal))
            return LinkInvalido;
        if (ahora > usuario.VenceRecuperacion)
            return "El link venció. Pedí otro desde «¿Olvidaste tu contraseña?».";
        return null;
    }

    public static void ExigirClave(string? clave)
    {
        if (string.IsNullOrWhiteSpace(clave) || clave.Length < ServicioRegistro.LargoMinimoClave)
            throw new ErrorNegocio($"La contraseña debe tener al menos {ServicioRegistro.LargoMinimoClave} caracteres.");
    }

    /// <summary>El link no sirve dos veces y las sesiones abiertas se cortan.</summary>
    public static void Completar(Usuario usuario)
    {
        usuario.TokenRecuperacion = null;
        usuario.VenceRecuperacion = null;
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        ServicioUsuarios.ClaveCambiada(usuario);
    }
}
