namespace StockControl.Dominio;

public class InformeVenta
{
    public int IdInformeVenta { get; set; }
    public int IdLocal { get; set; }
    public int? IdUsuario { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = "";
    public bool MultipleMetodoDePago { get; set; }
    public bool DetalleAdjunto { get; set; } = true;
    public decimal Descuento { get; set; }
    /// <summary>Importe descontado por el medio de pago. Queda guardado: editar el medio después no cambia esta venta.</summary>
    public decimal DescuentoMedio { get; set; }
    public int DescuentoMedioPorcentaje { get; set; }
    public decimal Subtotal { get; set; }
    public string PrecioCosto { get; set; } = "NO";
    /// <summary>Puesto de caja (1…N). Obligatorio al cobrar.</summary>
    public int NroCaja { get; set; }
    /// <summary>Nació al entregar un pedido de la tienda online (sigue entrando en esa caja).</summary>
    public bool EsTiendaOnline { get; set; }
    /// <summary>Null = venta abierta; set = incluida en ese cierre.</summary>
    public int? IdCierre { get; set; }
    /// <summary>Id generado en la caja (Guid). Evita duplicar la venta si se reenvía. Único por local.</summary>
    public Guid? IdVentaCliente { get; set; }
    /// <summary>Entró sin conexión y el stock rígido no alcanzaba.</summary>
    public bool StockEnConflicto { get; set; }
    public string? NotaRevision { get; set; }
    public bool ParaRevisar => StockEnConflicto || !string.IsNullOrEmpty(NotaRevision);

    public void MarcarRevisada()
    {
        StockEnConflicto = false;
        NotaRevision = null;
    }

    public List<InformeVentaDetalle> Detalles { get; set; } = new();
    public List<PagoVenta> Pagos { get; set; } = new();
}
