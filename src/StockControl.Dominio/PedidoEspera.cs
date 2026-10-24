namespace StockControl.Dominio;

/// <summary>Carrito guardado sin cobrar. Es del puesto de caja que lo guardó.</summary>
public class PedidoEspera
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int NroCaja { get; set; }
    public int? IdUsuario { get; set; }
    public string Nota { get; set; } = "";
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public int DescuentoPorcentaje { get; set; }
    public bool AlCosto { get; set; }
    public List<LineaPedidoEspera> Lineas { get; set; } = new();
}

public class LineaPedidoEspera
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdPedidoEspera { get; set; }
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class PedidoYaCobrado : ErrorNegocio
{
    public PedidoYaCobrado() : base("Ese pedido ya se cobró desde otra pestaña.") { }
}
