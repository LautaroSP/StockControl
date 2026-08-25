namespace StockControl.Dominio;

public static class PrecioVenta
{
    /// <summary>
    /// Descuento sobre el margen (precio − costo), no sobre el precio entero.
    /// Ej: 1500 − 1000 = 500; 50% → 1500 − 250 = 1250.
    /// </summary>
    public static decimal AplicarDescuento(decimal precioLista, decimal costo, int descuentoPorcentaje)
    {
        if (descuentoPorcentaje <= 0)
            return Math.Round(precioLista, 2, MidpointRounding.AwayFromZero);

        var margen = Math.Max(0, precioLista - costo);
        return Math.Round(
            precioLista - (margen * descuentoPorcentaje / 100m),
            2,
            MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Precio de una línea: sector/genérico no se tocan; al costo gana al descuento.
    /// </summary>
    public static decimal PrecioUnitario(
        decimal precioLista,
        decimal costo,
        int descuentoPorcentaje,
        bool alCosto,
        bool precioLibre,
        decimal precioEnCaja)
    {
        if (precioLibre)
            return Math.Round(precioEnCaja, 2, MidpointRounding.AwayFromZero);
        if (alCosto)
            return Math.Round(costo, 2, MidpointRounding.AwayFromZero);
        return AplicarDescuento(precioLista, costo, descuentoPorcentaje);
    }
}
