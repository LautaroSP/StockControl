using System.ComponentModel.DataAnnotations;

namespace StockControl.Api.Fiscal;

public class ConfiguracionFiscalPedido
{
    [Required, MaxLength(13)]
    public string Cuit { get; set; } = "";

    [Range(1, 99999)]
    public int PuntoVenta { get; set; }

    public bool Activa { get; set; }

    public bool FacturaGlobalPorDefecto { get; set; }
}

public class FacturarCajaPedido
{
    /// <summary>venta | global</summary>
    [Required, MaxLength(10)]
    public string Modo { get; set; } = "venta";

    /// <summary>Opcional: facturar solo estas ventas de la caja.</summary>
    [MaxLength(500)]
    public List<int>? IdsVentas { get; set; }
}
