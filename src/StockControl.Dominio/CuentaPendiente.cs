namespace StockControl.Dominio;

public static class EstadoCuenta
{
    public const string Abierta = "abierta";
    public const string Pagada = "pagada";
    public const string Incobrable = "incobrable";
}

/// <summary>Ticket fiado. No es venta: cada pago crea la venta en la caja de ese momento.</summary>
public class CuentaPendiente
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdCliente { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public int NroCaja { get; set; }
    public int? IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = "";
    public decimal Importe { get; set; }
    public decimal Saldo { get; set; }
    public decimal? GananciaEstimada { get; set; }
    public string Estado { get; set; } = EstadoCuenta.Abierta;
    public DateTimeOffset? FechaIncobrable { get; set; }
    public string UsuarioIncobrable { get; set; } = "";
    public List<LineaCuenta> Lineas { get; set; } = new();
    public List<PagoCuenta> Pagos { get; set; } = new();
}

public class LineaCuenta : ILineaVendida
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdCuentaPendiente { get; set; }
    public int? IdProducto { get; set; }
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

public class PagoCuenta
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdCuentaPendiente { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    public decimal Importe { get; set; }
    public int? IdInformeVenta { get; set; }
    public int? IdUsuario { get; set; }
}

public class DeudaCliente
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = "";
    public decimal Saldo { get; set; }
    public int Cuentas { get; set; }
}
