namespace StockControl.Dominio;

public class AumentoHistorial
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public DateTimeOffset Fecha { get; set; }
    public string Usuario { get; set; } = "";
    public string TipoPrecio { get; set; } = TipoAumento.Ninguno;
    public decimal ValorPrecio { get; set; }
    public string TipoCosto { get; set; } = TipoAumento.Ninguno;
    public decimal ValorCosto { get; set; }
    public int RedondeoPrecio { get; set; }
    public bool Deshecho { get; set; }
    public DateTimeOffset? FechaDeshecho { get; set; }
    public string? UsuarioDeshizo { get; set; }
    public List<AumentoHistorialItem> Items { get; set; } = [];
}
