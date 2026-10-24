namespace StockControl.Dominio;

public class LineaPedidoCargada
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Costo { get; set; }
    public bool Sector { get; set; }
    public bool Generico { get; set; }
    public decimal? PrecioAnterior { get; set; }
}

public class PedidoCargado
{
    public int Id { get; set; }
    public string Nota { get; set; } = "";
    public int DescuentoPorcentaje { get; set; }
    public bool AlCosto { get; set; }
    public List<LineaPedidoCargada> Lineas { get; } = new();
    public List<string> Faltantes { get; } = new();
}

public static class ServicioPedidosEspera
{
    public const int LargoNota = 80;
    public const int MaximoPorPuesto = 50;

    public static PedidoEspera Guardar(
        int idLocal,
        int nroCaja,
        int? idUsuario,
        string nota,
        IReadOnlyList<ItemCarrito> items,
        int descuentoPorcentaje,
        bool alCosto)
    {
        if (items.Count == 0)
            throw new ErrorNegocio("Un carrito vacío no se guarda.");
        if (items.Any(i => i.Cantidad <= 0))
            throw new ErrorNegocio("Cada producto necesita una cantidad mayor a cero.");
        if (items.Any(i => i.Codigo.Length > 80 || i.Nombre.Length > 200))
            throw new ErrorNegocio("Hay un producto con código o nombre demasiado largo.");
        var limpia = nota.Trim();
        if (limpia.Length > LargoNota)
            throw new ErrorNegocio($"La nota admite hasta {LargoNota} caracteres.");

        return new PedidoEspera
        {
            IdLocal = idLocal,
            NroCaja = nroCaja,
            IdUsuario = idUsuario,
            Nota = limpia,
            DescuentoPorcentaje = descuentoPorcentaje,
            AlCosto = alCosto,
            Lineas = items.Select(i => new LineaPedidoEspera
            {
                IdLocal = idLocal,
                IdProducto = i.IdProducto,
                Codigo = i.Codigo,
                Nombre = i.Nombre,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList()
        };
    }

    public static IQueryable<PedidoEspera> DelPuesto(this IQueryable<PedidoEspera> pedidos, int nroCaja) =>
        pedidos.Where(p => p.NroCaja == nroCaja);

    public static void ExigirLugar(int guardadosEnPuesto)
    {
        if (guardadosEnPuesto >= MaximoPorPuesto)
            throw new ErrorNegocio($"Este puesto ya tiene {MaximoPorPuesto} pedidos en espera. Borrá alguno.");
    }

    public static void ExigirBorrado(int borrados)
    {
        if (borrados == 0)
            throw new PedidoYaCobrado();
    }

    public static PedidoCargado Cargar(PedidoEspera pedido, IReadOnlyList<Producto> productosDelLocal)
    {
        var cargado = new PedidoCargado
        {
            Id = pedido.Id,
            Nota = pedido.Nota,
            DescuentoPorcentaje = pedido.DescuentoPorcentaje,
            AlCosto = pedido.AlCosto
        };
        foreach (var linea in pedido.Lineas)
        {
            var esGenerico = linea.Codigo.StartsWith("GENERIC-", StringComparison.OrdinalIgnoreCase);
            var producto = esGenerico ? null : productosDelLocal.FirstOrDefault(p => p.Id == linea.IdProducto);
            if (!esGenerico && producto == null)
            {
                cargado.Faltantes.Add(linea.Nombre);
                continue;
            }
            cargado.Lineas.Add(ArmarLinea(linea, producto));
        }
        return cargado;
    }

    private static LineaPedidoCargada ArmarLinea(LineaPedidoEspera linea, Producto? producto)
    {
        var precioLibre = producto == null || producto.ProductoSector;
        var precio = precioLibre ? linea.PrecioUnitario : producto!.Precio;
        return new LineaPedidoCargada
        {
            IdProducto = linea.IdProducto,
            Codigo = linea.Codigo,
            Nombre = linea.Nombre,
            Cantidad = linea.Cantidad,
            PrecioUnitario = precio,
            Costo = producto?.Costo ?? 0,
            Sector = producto?.ProductoSector ?? false,
            Generico = producto == null,
            PrecioAnterior = precio != linea.PrecioUnitario ? linea.PrecioUnitario : null
        };
    }
}
