namespace StockControl.Dominio;

/// <summary>Pedido que arma un comprador desde la tienda pública. Retiro en el local.</summary>
public class PedidoTienda
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int Numero { get; set; }
    public string HashCodigo { get; set; } = "";
    public string Estado { get; set; } = ServicioPedidosTienda.Nuevo;
    public string Nombre { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public DateTimeOffset Creado { get; set; }
    public DateTimeOffset? Actualizado { get; set; }
    public DateTimeOffset? Cancelado { get; set; }
    public DateTimeOffset? Entregado { get; set; }
    public int? IdInformeVenta { get; set; }
    public List<LineaPedidoTienda> Lineas { get; set; } = new();
}

public class LineaPedidoTienda
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdPedidoTienda { get; set; }
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubTotal { get; set; }
}
