namespace StockControl.Dominio;

/// <summary>Lo que el usuario quiere recibir en ese local. Sin fila, valen los valores por defecto.</summary>
public class PreferenciaAviso
{
    public int Id { get; set; }
    public int IdLocal { get; set; }
    public int IdUsuario { get; set; }
    public bool Sonido { get; set; } = true;
    public bool PushNuevos { get; set; } = true;
    public bool PushCancelados { get; set; } = true;
}
