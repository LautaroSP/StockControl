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
    /// <summary>Lo que paga el comprador: el subtotal, más el envío, más la gestión si va a su cargo.</summary>
    public decimal Total { get; set; }
    public string Modalidad { get; set; } = ServicioEnvioTienda.Retiro;
    public string DireccionEnvio { get; set; } = "";
    public decimal? LatitudEnvio { get; set; }
    public decimal? LongitudEnvio { get; set; }
    public decimal? DistanciaKm { get; set; }
    /// <summary>Congelado al crear. 0 si es retiro o si el envío es gratis.</summary>
    public decimal Envio { get; set; }
    /// <summary>recorrido o recta. Vacío en un retiro.</summary>
    public string OrigenDistancia { get; set; } = "";
    public string MedioPago { get; set; } = ServicioPagoPedidoTienda.Coordinar;
    public string EstadoPago { get; set; } = ServicioPagoPedidoTienda.SinPago;
    /// <summary>Congelada al crear el pedido. Es de la plataforma, no venta del local.</summary>
    public decimal Gestion { get; set; }
    public bool GestionACargoComprador { get; set; }
    public string UrlPagoMp { get; set; } = "";
    public string? IdPagoMp { get; set; }
    /// <summary>Congelado al crear el pedido. Null: el pago no vence.</summary>
    public DateTimeOffset? VencePago { get; set; }
    public DateTimeOffset? Pagado { get; set; }
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
