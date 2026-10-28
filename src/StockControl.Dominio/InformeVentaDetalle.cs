namespace StockControl.Dominio;

public class InformeVentaDetalle : ILineaVendida
{
    public int IdInformeVentaDetalle { get; set; }
    public int IdLocal { get; set; }
    public int IdInformeVenta { get; set; }
    public int? IdProducto { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal? Costo { get; set; }
    public decimal Precio { get; set; }
    public decimal SubTotal { get; set; }
    public string DescripcionOferta { get; set; } = "";
    public decimal? PrecioBase { get; set; }
    public decimal AhorroOferta { get; set; }
    public decimal AhorroDescuento { get; set; }
    public decimal AhorroCombo { get; set; }
    public string NombreCombo { get; set; } = "";

    public static InformeVentaDetalle Desde(LineaVenta l, int idLocal) => new()
    {
        IdLocal = idLocal,
        IdProducto = l.IdProducto == 0 ? null : l.IdProducto,
        Codigo = l.Codigo,
        Nombre = l.Nombre,
        Cantidad = l.Cantidad,
        Precio = l.Precio,
        Costo = l.Costo,
        SubTotal = l.SubTotal,
        DescripcionOferta = l.DescripcionOferta,
        PrecioBase = l.PrecioBase,
        AhorroOferta = l.AhorroOferta,
        AhorroDescuento = l.AhorroDescuento,
        AhorroCombo = l.AhorroCombo,
        NombreCombo = l.NombreCombo
    };
}
