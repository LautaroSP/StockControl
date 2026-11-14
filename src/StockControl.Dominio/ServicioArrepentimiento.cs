using System.Security.Cryptography;

namespace StockControl.Dominio;

public static class ServicioArrepentimiento
{
    public static Arrepentimiento Nuevo(string nombre, string correo, string local, string mensaje, DateTimeOffset ahora) =>
        Armar(null, nombre, correo, local, mensaje, ahora);

    public static Arrepentimiento DeLaTienda(int idLocal, string nombreLocal, string nombre, string correo, string mensaje, DateTimeOffset ahora) =>
        Armar(idLocal, nombre, correo, nombreLocal, mensaje, ahora);

    private static Arrepentimiento Armar(int? idLocal, string nombre, string correo, string local, string mensaje, DateTimeOffset ahora)
    {
        var limpio = nombre.Trim();
        var mail = correo.Trim().ToLowerInvariant();
        var lugar = local.Trim();
        var texto = mensaje.Trim();
        if (limpio.Length == 0 || mail.Length == 0)
            throw new ErrorNegocio("Completá nombre y mail.");
        if (limpio.Length > 120 || mail.Length > 200 || lugar.Length > 200 || texto.Length > 2000)
            throw new ErrorNegocio("El pedido es demasiado largo.");
        if (!ServicioRegistro.EsMail(mail))
            throw new ErrorNegocio("El mail no es válido.");
        return new Arrepentimiento
        {
            Codigo = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)),
            Nombre = limpio,
            Correo = mail,
            Local = lugar,
            Mensaje = texto,
            Estado = Arrepentimiento.Pendiente,
            Creado = ahora,
            IdLocal = idLocal
        };
    }
}
