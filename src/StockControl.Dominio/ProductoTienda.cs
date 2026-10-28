namespace StockControl.Dominio;

/// <summary>Lo único que sale de un producto hacia la tienda pública: sin costo ni cantidad.</summary>
public sealed record ProductoTienda(
    int Id, string Codigo, string Nombre, decimal Precio, int? IdCategoria, bool HayStock,
    IReadOnlyList<ImagenTienda> Imagenes);
