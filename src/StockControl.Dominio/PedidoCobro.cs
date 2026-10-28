namespace StockControl.Dominio;

public class ItemCarrito
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class LineaVenta : ILineaVendida
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal? Costo { get; set; }
    public decimal SubTotal { get; set; }
    public string DescripcionOferta { get; set; } = "";
    public decimal? PrecioBase { get; set; }
    public decimal AhorroOferta { get; set; }
    public decimal AhorroDescuento { get; set; }
    public decimal AhorroCombo { get; set; }
    public string NombreCombo { get; set; } = "";
}

public class PedidoCobro
{
    public required List<Producto> ProductosDelLocal { get; init; }
    public required List<ItemCarrito> Items { get; init; }
    public int DescuentoPorcentaje { get; init; }
    public bool CobrarAlCosto { get; init; }
    public bool StockRigido { get; init; }
    /// <summary>Unidades apartadas por pedidos de la tienda: con stock rígido no se venden en caja.</summary>
    public IReadOnlyDictionary<int, decimal> Reservado { get; init; } = new Dictionary<int, decimal>();
    public string MetodoPago { get; init; } = "Efectivo";
    public IReadOnlyList<Oferta> Ofertas { get; init; } = [];
    public IReadOnlyList<OfertaCombo> Combos { get; init; } = [];
    public bool SinOfertas { get; init; }
    public DateOnly Hoy { get; init; }
}

public class ResultadoCobro
{
    public List<LineaVenta> Lineas { get; } = new();
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = "";
    public decimal Descuento { get; set; }
    public string PrecioCosto { get; set; } = "NO";
}
