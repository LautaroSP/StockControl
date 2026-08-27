using System.ComponentModel.DataAnnotations;

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

    [Required, MaxLength(80)]
    public string MetodoPago { get; set; } = "Efectivo";

    [Range(0, 100)]
    public int DescuentoPorcentaje { get; set; }

    public bool CobrarAlCosto { get; set; }
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
