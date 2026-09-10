namespace StockControl.Dominio;

public class Carrito
{
    public List<ItemCarrito> Items { get; } = new();
    public bool DescuentoActivo { get; set; }
    public int Descuento { get; set; }
    public bool AlCosto { get; set; }
    public bool ImprimirTicket { get; set; }

    public void Vaciar()
    {
        Items.Clear();
        DescuentoActivo = false;
        Descuento = 0;
        AlCosto = false;
    }
}

public static class ServicioCarritos
{
    public const int Fijos = 4;

    public static List<Carrito> Iniciar() =>
        Enumerable.Range(0, Fijos).Select(_ => new Carrito()).ToList();

    public static int Crear(IList<Carrito> carritos)
    {
        carritos.Add(new Carrito());
        return carritos.Count - 1;
    }

    public static bool PuedeCerrar(int indice) => indice >= Fijos;

    public static string Titulo(int indice)
    {
        var n = indice + 1;
        return indice < Fijos ? $"{n} F{n}" : n.ToString();
    }

    public static int Atajo(IReadOnlyList<Carrito> carritos, int teclaF, int indiceActual)
    {
        var indice = teclaF - 1;
        if (indice < 0 || indice >= Fijos || indice >= carritos.Count)
            return indiceActual;
        return indice;
    }

    public static int Cerrar(IList<Carrito> carritos, int indice, bool confirmar)
    {
        if (indice < 0 || indice >= carritos.Count)
            throw new ErrorNegocio("El carrito no existe.");
        if (indice < Fijos)
            throw new ErrorNegocio("Los primeros cuatro carritos no se cierran.");
        if (carritos[indice].Items.Count > 0 && !confirmar)
            throw new ErrorNegocio("Este carrito tiene productos.");

        carritos.RemoveAt(indice);
        return Math.Min(indice, carritos.Count - 1);
    }

    public static int AlCobrar(IList<Carrito> carritos, int indice)
    {
        if (indice < 0 || indice >= carritos.Count)
            throw new ErrorNegocio("El carrito no existe.");
        if (indice >= Fijos)
            return Cerrar(carritos, indice, confirmar: true);

        carritos[indice].Vaciar();
        return indice;
    }
}
