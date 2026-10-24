namespace StockControl.Dominio;

public static class ServicioVinculosProveedor
{
    public const int MaxNombrePack = 30;
    public const int MaxUnidadesPack = 10000;

    /// <summary>Por unidad queda sin nombre y con 1. Un pack necesita nombre y más de una unidad.</summary>
    public static (string Nombre, int Unidades) NormalizarPack(string? nombre, int unidades)
    {
        var limpio = (nombre ?? "").Trim();
        if (limpio.Length == 0 && unidades <= 1)
            return ("", 1);
        if (limpio.Length == 0)
            throw new ErrorNegocio("Poné el nombre del pack (Pack, Caja, Bulto u otro).");
        if (limpio.Length > MaxNombrePack)
            throw new ErrorNegocio($"El nombre del pack tiene hasta {MaxNombrePack} letras.");
        if (unidades is < 2 or > MaxUnidadesPack)
            throw new ErrorNegocio($"Un pack lleva de 2 a {MaxUnidadesPack} unidades.");
        return (limpio, unidades);
    }

    /// <summary>
    /// Crea el vínculo si el producto todavía no está en la lista del proveedor.
    /// Si ya estaba (por vínculo o por el proveedor del catálogo) y se pide por unidad, no hace falta otro.
    /// </summary>
    public static ProductoProveedor? Asegurar(
        IEnumerable<ProductoProveedor> vinculos, Producto producto, int idProveedor, string nombrePack, int unidadesPack)
    {
        if (vinculos.Any(v => v.IdProducto == producto.Id && v.IdProveedor == idProveedor))
            return null;
        var (nombre, unidades) = NormalizarPack(nombrePack, unidadesPack);
        if (producto.IdProveedor == idProveedor && unidades == 1)
            return null;
        return new ProductoProveedor
        {
            IdLocal = producto.IdLocal,
            IdProducto = producto.Id,
            IdProveedor = idProveedor,
            NombrePack = nombre,
            UnidadesPack = unidades
        };
    }

    /// <summary>Cambia el pack de un vínculo existente o crea uno nuevo.</summary>
    public static ProductoProveedor Fijar(
        ProductoProveedor? existente, Producto producto, int idProveedor, string nombrePack, int unidadesPack)
    {
        var (nombre, unidades) = NormalizarPack(nombrePack, unidadesPack);
        var vinculo = existente ?? new ProductoProveedor { IdLocal = producto.IdLocal, IdProducto = producto.Id, IdProveedor = idProveedor };
        vinculo.NombrePack = nombre;
        vinculo.UnidadesPack = unidades;
        return vinculo;
    }
}
