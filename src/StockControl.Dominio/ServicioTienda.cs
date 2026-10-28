using System.Text.RegularExpressions;

namespace StockControl.Dominio;

public static class ServicioTienda
{
    public const string Inactiva = "inactiva";
    public const string Activa = "activa";
    public const string Pausada = "pausada";

    public const string VistaCatalogo = "catalogo";
    public const string VistaPausada = "pausada";
    public const string VistaCerrada = "cerrada";

    public const int LargoMinimoAlias = 3;
    public const int LargoMaximoAlias = 40;
    public const int LargoMaximoDato = 200;

    private static readonly Regex FormatoAlias = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    public static readonly IReadOnlySet<string> AliasReservados = new HashSet<string>
    {
        "admin", "api", "app", "ayuda", "caja", "cajas", "confirmar", "configuracion", "contacto",
        "locales", "login", "mercadopago", "productos", "registro", "salud", "soporte",
        "stockcontrol", "tienda", "tiendas", "ventas", "www"
    };

    private static readonly string[] Estados = [Inactiva, Activa, Pausada];

    /// <summary>Vacío = sin alias. Minúsculas, números y guiones; sin guion al principio ni al final.</summary>
    public static string? NormalizarAlias(string? alias)
    {
        var valor = (alias ?? "").Trim().ToLowerInvariant();
        if (valor.Length == 0) return null;
        if (valor.Length < LargoMinimoAlias || valor.Length > LargoMaximoAlias)
            throw new ErrorNegocio($"El alias tiene que tener entre {LargoMinimoAlias} y {LargoMaximoAlias} caracteres.");
        if (!FormatoAlias.IsMatch(valor))
            throw new ErrorNegocio("El alias solo admite letras sin acento, números y guiones. Sin espacios ni guion al principio o al final.");
        if (AliasReservados.Contains(valor))
            throw new ErrorNegocio("Ese alias está reservado. Elegí otro.");
        return valor;
    }

    public static void AsignarAlias(Local local, string? alias, bool tomadoPorOtro)
    {
        if (alias != null && tomadoPorOtro)
            throw new ErrorNegocio("Ese alias ya lo usa otra tienda. Elegí otro.");
        local.AliasTienda = alias;
    }

    public static void CambiarEstado(Local local, string? estado)
    {
        var valor = (estado ?? "").Trim().ToLowerInvariant();
        if (!Estados.Contains(valor))
            throw new ErrorNegocio("Estado de tienda inválido.");
        if (valor != Inactiva && local.AliasTienda == null)
            throw new ErrorNegocio("Elegí un alias para activar la tienda.");
        local.EstadoTienda = valor;
    }

    public static void AsignarDatos(Local local, string? direccion, string? horario, string? whatsApp, bool haceEnvios)
    {
        local.Direccion = Recortar(direccion, "La dirección");
        local.Horario = Recortar(horario, "El horario");
        local.WhatsApp = string.IsNullOrWhiteSpace(whatsApp) ? "" : ServicioOrdenCompra.NormalizarTelefono(whatsApp);
        local.HaceEnvios = haceEnvios;
    }

    /// <summary>Las dos o ninguna. Latitud entre -90 y 90; longitud entre -180 y 180.</summary>
    public static void AsignarUbicacion(Local local, decimal? latitud, decimal? longitud)
    {
        if (latitud.HasValue != longitud.HasValue)
            throw new ErrorNegocio("La ubicación necesita latitud y longitud.");
        if (latitud is < -90 or > 90)
            throw new ErrorNegocio("La latitud tiene que estar entre -90 y 90.");
        if (longitud is < -180 or > 180)
            throw new ErrorNegocio("La longitud tiene que estar entre -180 y 180.");
        local.Latitud = latitud is { } lat ? Math.Round(lat, 6) : null;
        local.Longitud = longitud is { } lon ? Math.Round(lon, 6) : null;
    }

    public static bool MuestraMapa(Local local) => local.Latitud.HasValue && local.Longitud.HasValue;

    /// <summary>Null = no resuelve (404). El abono manda sobre la pausa del dueño.</summary>
    public static string? Vista(Local local, DateTimeOffset ahora)
    {
        if (!local.Activo || local.AliasTienda == null || local.EstadoTienda == Inactiva)
            return null;
        if (!ServicioAbono.Estado(local, ahora).PuedeOperarCaja)
            return VistaCerrada;
        return local.EstadoTienda == Pausada ? VistaPausada : VistaCatalogo;
    }

    private static string Recortar(string? valor, string campo)
    {
        var texto = (valor ?? "").Trim();
        if (texto.Length > LargoMaximoDato)
            throw new ErrorNegocio($"{campo} admite hasta {LargoMaximoDato} caracteres.");
        return texto;
    }
}
