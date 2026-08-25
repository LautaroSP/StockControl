namespace StockControl.Dominio;

public class Configuracion
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public string Clave { get; set; } = "";
    public string Valor { get; set; } = "";
}
