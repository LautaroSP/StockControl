namespace StockControl.Dominio;

/// <summary>
/// Línea cobrada con sus ahorros. PrecioBase × Cantidad − ahorros = SubTotal.
/// PrecioBase es null en ventas anteriores a los ahorros: ahí solo vale SubTotal.
/// </summary>
public interface ILineaVendida
{
    string Codigo { get; }
    string Nombre { get; }
    decimal Cantidad { get; }
    decimal Precio { get; }
    decimal SubTotal { get; }
    string DescripcionOferta { get; }
    decimal? PrecioBase { get; }
    decimal AhorroOferta { get; }
    decimal AhorroDescuento { get; }
    decimal AhorroCombo { get; }
    string NombreCombo { get; }
}
