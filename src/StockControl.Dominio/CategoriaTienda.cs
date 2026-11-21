namespace StockControl.Dominio;

/// <summary>Categoría con productos publicados (o en hijos) que se ofrece como filtro en la tienda.</summary>
public sealed record CategoriaTienda(int Id, string Nombre, int? IdPadre, int Cantidad);
