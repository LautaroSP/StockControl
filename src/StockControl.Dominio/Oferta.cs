namespace StockControl.Dominio;

public static class TipoOferta
{
    public const string Descuento = "descuento";
    public const string LlevaPaga = "llevaPaga";
    /// <summary>Desde N unidades, cada una sale PrecioUnidad.</summary>
    public const string PrecioDesde = "precioDesde";
    /// <summary>Con exactamente N unidades, cada una sale PrecioUnidad.</summary>
    public const string PrecioExacto = "precioExacto";
}

/// <summary>Oferta de un producto del local entre dos días de Argentina, inclusive. Sin fin = FechaHasta en DateOnly.MaxValue.</summary>
public class Oferta
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdProducto { get; set; }
    public string Tipo { get; set; } = TipoOferta.Descuento;
    public int Porcentaje { get; set; }
    public int Lleva { get; set; }
    public int Paga { get; set; }
    public decimal PrecioUnidad { get; set; }
    public DateOnly FechaDesde { get; set; }
    public DateOnly FechaHasta { get; set; }
    public bool AnulaConDescuentoCaja { get; set; }
    public bool AnulaConAlCosto { get; set; } = true;
    public bool PermiteNoAplicarEnCaja { get; set; }

    public bool SinFin => FechaHasta == DateOnly.MaxValue;
}
