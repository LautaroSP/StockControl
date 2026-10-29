using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace StockControl.Dominio;

public record DatosComprador(string? Nombre, string? Telefono, string? Email);

public record ItemPedidoTienda(int IdProducto, int Cantidad);

/// <summary>Maximo tiene valor solo con stock rígido y si se pidió más de lo disponible.</summary>
public record LineaCotizada(int IdProducto, string Codigo, string Nombre, int Cantidad, decimal PrecioUnitario, decimal SubTotal, int? Maximo);

public record CotizacionTienda(IReadOnlyList<LineaCotizada> Lineas, IReadOnlyList<int> NoDisponibles, decimal Total);

public record PedidoCreado(PedidoTienda Pedido, string Codigo);

public static class ServicioPedidosTienda
{
    public const string Nuevo = "nuevo";
    public const string Preparando = "preparando";
    public const string Listo = "listo";
    public const string Entregado = "entregado";
    public const string Cancelado = "cancelado";

    /// <summary>Estados que reservan stock: el pedido sigue abierto.</summary>
    public static readonly string[] Activos = [Nuevo, Preparando, Listo];

    private static readonly Dictionary<string, string[]> Transiciones = new()
    {
        [Nuevo] = [Preparando, Listo, Entregado, Cancelado],
        [Preparando] = [Listo, Entregado, Cancelado],
        [Listo] = [Entregado, Cancelado]
    };

    public const int MaximoLineas = 50;
    public const int MaximoCantidad = 99;
    public const int LargoMinimoNombre = 2;
    public const int LargoMaximoNombre = 80;
    public const int LargoMaximoEmail = 200;
    public const int LargoMaximoCodigo = 64;

    private static readonly Regex FormatoEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    /// <summary>Precios de lista del servidor. Del cliente solo toma producto y cantidad.</summary>
    public static CotizacionTienda Cotizar(
        int idLocal,
        IReadOnlyList<ItemPedidoTienda> items,
        IReadOnlyCollection<Producto> productos,
        IReadOnlyDictionary<int, decimal> reservado,
        bool stockRigido)
    {
        var lineas = new List<LineaCotizada>();
        var noDisponibles = new List<int>();
        foreach (var item in Unificar(items))
        {
            var producto = productos.FirstOrDefault(p => p.Id == item.IdProducto && EnTienda(p, idLocal));
            if (producto == null)
                noDisponibles.Add(item.IdProducto);
            else
                lineas.Add(Cotizar(producto, item.Cantidad, reservado, stockRigido));
        }
        return new CotizacionTienda(lineas, noDisponibles, lineas.Sum(l => l.SubTotal));
    }

    public static PedidoCreado Crear(
        Local local,
        DateTimeOffset ahora,
        int numero,
        DatosComprador datos,
        IReadOnlyList<ItemPedidoTienda> items,
        IReadOnlyCollection<Producto> productos,
        IReadOnlyDictionary<int, decimal> reservado,
        bool stockRigido)
    {
        ExigirAbierta(local, ahora);
        var pedido = new PedidoTienda
        {
            IdLocal = local.IdLocal,
            Numero = numero,
            Nombre = NormalizarNombre(datos.Nombre),
            Telefono = ServicioOrdenCompra.NormalizarTelefono(datos.Telefono),
            Email = NormalizarEmail(datos.Email),
            Creado = ahora
        };
        var cotizacion = Cotizar(local.IdLocal, items, productos, reservado, stockRigido);
        ExigirCompleta(cotizacion);
        pedido.Lineas.AddRange(cotizacion.Lineas.Select(l => ALinea(local.IdLocal, l)));
        pedido.Subtotal = cotizacion.Total;
        pedido.Total = cotizacion.Total;

        var codigo = GenerarCodigo();
        pedido.HashCodigo = HashDeCodigo(codigo);
        return new PedidoCreado(pedido, codigo);
    }

    public static void ExigirAbierta(Local local, DateTimeOffset ahora)
    {
        if (ServicioTienda.Vista(local, ahora) != ServicioTienda.VistaCatalogo)
            throw new ErrorNegocio("La tienda no está recibiendo pedidos en este momento.");
    }

    /// <summary>El comprador cancela solo un pedido nuevo. Cancelado deja de contar como reservado.</summary>
    public static void Cancelar(PedidoTienda pedido, DateTimeOffset ahora)
    {
        if (pedido.Estado == Cancelado)
            throw new ErrorNegocio("El pedido ya está cancelado.");
        if (pedido.Estado != Nuevo)
            throw new ErrorNegocio("El pedido ya no se puede cancelar.");
        CancelarDelLocal(pedido, ahora);
    }

    public static bool PuedeCancelar(PedidoTienda pedido) => pedido.Estado == Nuevo;

    /// <summary>El local cancela mientras el pedido siga abierto.</summary>
    public static void CancelarDelLocal(PedidoTienda pedido, DateTimeOffset ahora)
    {
        CambiarEstado(pedido, Cancelado, ahora);
        pedido.Cancelado = ahora;
    }

    /// <summary>Preparando o Listo. Entregar y cancelar tienen su propio paso.</summary>
    public static void Avanzar(PedidoTienda pedido, string? estado, DateTimeOffset ahora)
    {
        if (estado is not (Preparando or Listo))
            throw new ErrorNegocio("Ese estado no existe.");
        CambiarEstado(pedido, estado, ahora);
    }

    public static IReadOnlyList<string> Siguientes(string estado) =>
        Transiciones.TryGetValue(estado, out var siguientes) ? siguientes : [];

    public static bool EsActivo(string estado) => Activos.Contains(estado);

    internal static void CambiarEstado(PedidoTienda pedido, string estado, DateTimeOffset ahora)
    {
        if (!Siguientes(pedido.Estado).Contains(estado))
            throw new ErrorNegocio(pedido.Estado switch
            {
                Cancelado => "El pedido ya está cancelado.",
                Entregado => "El pedido ya se entregó.",
                _ => "El pedido no puede pasar a ese estado."
            });
        pedido.Estado = estado;
        pedido.Actualizado = ahora;
    }

    /// <summary>Unidades que los pedidos abiertos tienen apartadas, por producto.</summary>
    public static Dictionary<int, decimal> Reservado(IEnumerable<PedidoTienda> pedidos) =>
        pedidos.Where(p => EsActivo(p.Estado))
            .SelectMany(p => p.Lineas)
            .GroupBy(l => l.IdProducto)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Cantidad));

    public static decimal Disponible(Producto producto, IReadOnlyDictionary<int, decimal> reservado) =>
        producto.Cantidad - reservado.GetValueOrDefault(producto.Id);

    public static string GenerarCodigo() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string HashDeCodigo(string? codigo)
    {
        var valor = (codigo ?? "").Trim();
        if (valor.Length > LargoMaximoCodigo) valor = valor[..LargoMaximoCodigo];
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));
    }

    private static bool EnTienda(Producto producto, int idLocal) =>
        producto.IdLocal == idLocal && producto.Publicado && ServicioCatalogoTienda.PuedePublicar(producto);

    private static List<ItemPedidoTienda> Unificar(IReadOnlyList<ItemPedidoTienda> items)
    {
        if (items.Count == 0)
            throw new ErrorNegocio("El carrito está vacío.");
        var lineas = items.GroupBy(i => i.IdProducto)
            .Select(g => new ItemPedidoTienda(g.Key, g.Sum(i => i.Cantidad)))
            .ToList();
        if (lineas.Count > MaximoLineas)
            throw new ErrorNegocio($"Un pedido admite hasta {MaximoLineas} productos distintos.");
        return lineas;
    }

    private static LineaCotizada Cotizar(Producto producto, int cantidad, IReadOnlyDictionary<int, decimal> reservado, bool stockRigido)
    {
        if (cantidad < 1 || cantidad > MaximoCantidad)
            throw new ErrorNegocio($"La cantidad de «{producto.Nombre}» tiene que estar entre 1 y {MaximoCantidad}.");
        var disponible = Math.Max(0, (int)Math.Floor(Disponible(producto, reservado)));
        int? maximo = stockRigido && cantidad > disponible ? disponible : null;
        var subTotal = Math.Round(producto.Precio * cantidad, 2, MidpointRounding.AwayFromZero);
        return new LineaCotizada(producto.Id, producto.Codigo, producto.Nombre, cantidad, producto.Precio, subTotal, maximo);
    }

    private static void ExigirCompleta(CotizacionTienda cotizacion)
    {
        if (cotizacion.NoDisponibles.Count > 0)
            throw new ErrorNegocio("Un producto del carrito ya no está disponible en la tienda.");
        var sinStock = cotizacion.Lineas.FirstOrDefault(l => l.Maximo != null);
        if (sinStock != null)
            throw new ErrorNegocio(sinStock.Maximo == 0
                ? $"«{sinStock.Nombre}» no tiene stock."
                : $"De «{sinStock.Nombre}» quedan {sinStock.Maximo}.");
    }

    private static LineaPedidoTienda ALinea(int idLocal, LineaCotizada linea) => new()
    {
        IdLocal = idLocal,
        IdProducto = linea.IdProducto,
        Codigo = linea.Codigo,
        Nombre = linea.Nombre,
        Cantidad = linea.Cantidad,
        PrecioUnitario = linea.PrecioUnitario,
        SubTotal = linea.SubTotal
    };

    private static string NormalizarNombre(string? nombre)
    {
        var valor = (nombre ?? "").Trim();
        if (valor.Length < LargoMinimoNombre || valor.Length > LargoMaximoNombre)
            throw new ErrorNegocio($"El nombre tiene que tener entre {LargoMinimoNombre} y {LargoMaximoNombre} caracteres.");
        return valor;
    }

    private static string NormalizarEmail(string? email)
    {
        var valor = (email ?? "").Trim();
        if (valor.Length > LargoMaximoEmail || !FormatoEmail.IsMatch(valor))
            throw new ErrorNegocio("El mail no es válido.");
        return valor;
    }
}
