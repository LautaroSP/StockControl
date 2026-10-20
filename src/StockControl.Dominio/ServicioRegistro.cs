using System.Net.Mail;

namespace StockControl.Dominio;

public class RegistroPedido
{
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string NombreUsuario { get; set; } = "";
    public string NombreLocal { get; set; } = "";
    public string Modalidad { get; set; } = ServicioRegistro.Prueba;
}

public static class ServicioRegistro
{
    public const string Prueba = "prueba";
    public const string Pagar = "pagar";
    public const int LargoMinimoClave = 8;

    public static readonly string[] MediosIniciales = ["Efectivo", "Mercado Pago", "Transferencia"];

    public static void Validar(RegistroPedido pedido, string clave)
    {
        if (Vacio(pedido.Nombre) || Vacio(pedido.Email) || Vacio(pedido.NombreUsuario) || Vacio(pedido.NombreLocal))
            throw new ErrorNegocio("Completá nombre, mail, usuario y nombre del local.");
        if (pedido.Nombre.Trim().Length > 120 || pedido.NombreUsuario.Trim().Length > 80
            || pedido.Email.Trim().Length > 200 || pedido.NombreLocal.Trim().Length > 200)
            throw new ErrorNegocio("Algún dato es demasiado largo.");
        if (!EsMail(pedido.Email.Trim()))
            throw new ErrorNegocio("El mail no es válido.");
        if (string.IsNullOrEmpty(clave) || clave.Length < LargoMinimoClave || clave.Length > 200)
            throw new ErrorNegocio($"La contraseña debe tener al menos {LargoMinimoClave} caracteres.");
        if (pedido.Modalidad is not (Prueba or Pagar))
            throw new ErrorNegocio("Elegí probar gratis o pagar.");
    }

    public static (Usuario Dueno, Local Local) Armar(RegistroPedido pedido, DateTimeOffset ahora)
    {
        var dueno = new Usuario
        {
            Nombre = pedido.Nombre.Trim(),
            Email = pedido.Email.Trim().ToLowerInvariant(),
            NombreUsuario = pedido.NombreUsuario.Trim(),
            Rol = Roles.Dueno,
            Activo = true
        };
        var prueba = pedido.Modalidad == Prueba;
        var local = new Local
        {
            Nombre = pedido.NombreLocal.Trim(),
            EstadoAbono = prueba ? ServicioAbono.Trial : ServicioAbono.AlDia,
            Vence = prueba ? ServicioAbono.VenceTrial(ahora) : null
        };
        return (dueno, local);
    }

    private static bool Vacio(string? texto) => string.IsNullOrWhiteSpace(texto);

    public static bool EsMail(string texto)
    {
        try
        {
            return new MailAddress(texto).Address == texto && texto.Contains('.');
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
