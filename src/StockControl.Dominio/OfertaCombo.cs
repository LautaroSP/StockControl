namespace StockControl.Dominio;

/// <summary>Combo: varias cantidades de productos distintos por un precio total. Se repite por cada combo completo.</summary>
public class OfertaCombo
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public string Nombre { get; set; } = "";
    public decimal PrecioTotal { get; set; }
    public DateOnly FechaDesde { get; set; }
    public DateOnly FechaHasta { get; set; }
    public List<LineaOfertaCombo> Lineas { get; set; } = [];

    public bool SinFin => FechaHasta == DateOnly.MaxValue;
}

public class LineaOfertaCombo
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdOfertaCombo { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
}
