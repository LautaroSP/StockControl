using System.Globalization;
using System.Text;

namespace StockControl.Dominio;

public sealed class LineaOrdenPedida
{
    public int IdProducto { get; init; }
    public decimal Cantidad { get; init; }
}

public static class ServicioOrdenCompra
{
    private static readonly CultureInfo Ar = new("es-AR");

    public static OrdenCompra Armar(
        int idProveedor,
        int idLocal,
        IEnumerable<LineaOrdenPedida> lineas,
        IEnumerable<Producto> productosDelLocal,
        int? idUsuario,
        string nombreUsuario,
        DateTimeOffset fecha)
    {
        var pedidas = lineas.ToList();
        if (pedidas.Count == 0)
            throw new ErrorNegocio("La orden no tiene productos.");
        if (pedidas.Any(l => l.Cantidad <= 0))
            throw new ErrorNegocio("Cada línea tiene que tener cantidad mayor a 0.");

        var productos = productosDelLocal.ToDictionary(p => p.Id);
        var orden = new OrdenCompra
        {
            IdLocal = idLocal,
            IdProveedor = idProveedor,
            Fecha = fecha,
            IdUsuario = idUsuario,
            NombreUsuario = nombreUsuario
        };
        foreach (var g in pedidas.GroupBy(l => l.IdProducto))
        {
            if (!productos.TryGetValue(g.Key, out var p) || p.IdProveedor != idProveedor)
                throw new ErrorNegocio("Hay productos que no son de este proveedor.");
            orden.Lineas.Add(new OrdenCompraLinea
            {
                IdLocal = idLocal,
                IdProducto = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Cantidad = g.Sum(l => l.Cantidad)
            });
        }
        return orden;
    }

    /// <summary>
    /// Formato internacional de celular argentino para wa.me: 549 + área + número (10 dígitos), sin 0 ni 15.
    /// </summary>
    public static string NormalizarTelefono(string? telefono)
    {
        var d = new string((telefono ?? "").Where(char.IsDigit).ToArray());
        if (d.StartsWith("00")) d = d[2..];
        if (d.StartsWith("54")) d = d[2..];
        if (d.StartsWith('9') && d.Length == 11) d = d[1..];
        if (d.StartsWith('0')) d = d[1..];
        if (d.Length == 12) d = SacarQuince(d);
        if (d.Length != 10)
            throw new ErrorNegocio("El teléfono tiene que tener código de área y número (10 dígitos, sin 0 ni 15).");
        return "549" + d;
    }

    public static string TextoWhatsApp(string nombreLocal, string nombreProveedor, DateTimeOffset fecha, IEnumerable<OrdenCompraLinea> lineas)
    {
        var dia = TimeZoneInfo.ConvertTime(fecha, DiaArgentina.Zona).ToString("dd/MM/yyyy", Ar);
        var texto = new StringBuilder($"Hola {nombreProveedor}, pedido de {nombreLocal} ({dia}):");
        foreach (var l in lineas)
            texto.Append($"\n- {l.Cantidad.ToString("0.###", Ar)} x {l.Nombre} ({l.Codigo})");
        return texto.ToString();
    }

    public static string UrlWhatsApp(string telefono, string texto) =>
        $"https://wa.me/{NormalizarTelefono(telefono)}?text={Uri.EscapeDataString(texto)}";

    private static string SacarQuince(string d)
    {
        foreach (var posicion in new[] { 2, 3, 4 })
        {
            if (d.Substring(posicion, 2) == "15")
                return d.Remove(posicion, 2);
        }
        return d;
    }
}
