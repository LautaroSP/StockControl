namespace StockControl.Dominio;

public class ItemCarrito
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class LineaVenta
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal? Costo { get; set; }
    public decimal SubTotal { get; set; }
}

public class PedidoCobro
{
    public required List<Producto> ProductosDelLocal { get; init; }
    public required List<ItemCarrito> Items { get; init; }
    public int DescuentoPorcentaje { get; init; }
    public bool CobrarAlCosto { get; init; }
    public bool StockRigido { get; init; }
    public string MetodoPago { get; init; } = "Efectivo";
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
