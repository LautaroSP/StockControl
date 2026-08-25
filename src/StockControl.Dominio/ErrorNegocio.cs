namespace StockControl.Dominio;

public class ErrorNegocio : Exception
{
    public ErrorNegocio(string mensaje) : base(mensaje) { }
}
