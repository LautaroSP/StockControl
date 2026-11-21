namespace StockControl.Dominio;

/// <summary>Falla al hablar con ARCA. Transitoria = no sabemos si emitió (timeout, caída): hay que consultar antes de reintentar.</summary>
public class ExcepcionArca : Exception
{
    public bool Transitoria { get; }

    public ExcepcionArca(string mensaje, bool transitoria) : base(mensaje)
    {
        Transitoria = transitoria;
    }
}
