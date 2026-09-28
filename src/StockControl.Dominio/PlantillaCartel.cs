namespace StockControl.Dominio;

public class PlantillaCartel
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public string Tamano { get; set; } = "";
    public string TipoContenido { get; set; } = "";
    public byte[] Datos { get; set; } = [];
}
