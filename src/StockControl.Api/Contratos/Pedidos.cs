using System.ComponentModel.DataAnnotations;
using StockControl.Dominio;

namespace StockControl.Api.Contratos;

public class LoginPedido
{
    [Required, MaxLength(80)]
    public string Usuario { get; set; } = "";

    [Required, MinLength(4)]
    public string Clave { get; set; } = "";
}

public class ProductoPedido
{
    [Required, MaxLength(80)]
    public string Codigo { get; set; } = "";

    [Required, MaxLength(200)]
    public string Nombre { get; set; } = "";

    public decimal Cantidad { get; set; }
    public decimal Costo { get; set; }
    public decimal Precio { get; set; }
    public bool ProductoSector { get; set; }
    public int? IdGrupoProducto { get; set; }
}

public class GrupoPedido
{
    [Required, MaxLength(200)]
    public string NombreGrupo { get; set; } = "";

    public decimal Costo { get; set; }
    public decimal PrecioGrupo { get; set; }
    public decimal Ganancia { get; set; }
    public bool GananciaIndividual { get; set; }
}

public class MiembrosGrupoPedido
{
    public List<int> IdsProducto { get; set; } = new();
}

public class StockPedido
{
    public decimal Cantidad { get; set; }
}

public class VentaPedido
{
    [Required, MinLength(1)]
    public List<ItemVentaPedido> Items { get; set; } = new();

    [MaxLength(80)]
    public string MetodoPago { get; set; } = "";

    public List<PagoPedido> Pagos { get; set; } = new();

    [Range(0, 100)]
    public int DescuentoPorcentaje { get; set; }

    public bool CobrarAlCosto { get; set; }
}

public class PrecioProductoPedido
{
    [Range(0, 999999999)]
    public decimal Precio { get; set; }
}

public class UsuarioPedido
{
    [Required, MaxLength(120)]
    public string Nombre { get; set; } = "";

    [Required, MaxLength(80)]
    public string NombreUsuario { get; set; } = "";

    [MaxLength(20)]
    public string Rol { get; set; } = Roles.Empleado;

    public List<int> IdsLocal { get; set; } = new();

    [MinLength(4), MaxLength(200)]
    public string? Clave { get; set; }
}

public class ResetearClavePedido
{
    [Required, MinLength(4), MaxLength(200)]
    public string Clave { get; set; } = "";
}

public class ConfiguracionLocalPedido
{
    [Required, MaxLength(200)]
    public string NombreLocal { get; set; } = "";

    [Range(0.0001, 999999)]
    public decimal FactorGanancia { get; set; }

    [Range(0, 999999)]
    public decimal Iva { get; set; }

    public bool StockRigido { get; set; }

    [Range(0, 999999)]
    public decimal UmbralStockBajo { get; set; }

    public bool EmpleadoPuedeModificarPrecios { get; set; }

    [Required, MaxLength(10)]
    public string FormatoTicket { get; set; } = "POS-80";

    public bool ImprimirTicketAlCobrar { get; set; }

    [Range(1, 50)]
    public int CantidadCajas { get; set; } = 1;

    public bool RecalcularPrecios { get; set; }
}

public class PagoPedido
{
    public int IdMetodoPago { get; set; }

    [Range(0.01, 999999999)]
    public decimal Importe { get; set; }
}

public class MetodoPagoPedido
{
    [Required, MaxLength(80)]
    public string Descripcion { get; set; } = "";
}

public class ItemVentaPedido
{
    public int IdProducto { get; set; }

    [Required]
    public string Codigo { get; set; } = "";

    public string Nombre { get; set; } = "";

    [Range(0.001, 999999)]
    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }
}

public class CerrarCajaPedido
{
    public DateOnly? Fecha { get; set; }

    /// <summary>medio | usuario</summary>
    public string Desglose { get; set; } = "medio";

    public bool Todas { get; set; }
}

public class CantidadCajasPedido
{
    [Range(1, 50)]
    public int Cantidad { get; set; } = 1;
}

public class UnificarCajasPedido
{
    [Required, MinLength(2)]
    public List<int> IdsCierre { get; set; } = new();
}
