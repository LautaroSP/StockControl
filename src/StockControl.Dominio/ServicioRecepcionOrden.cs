namespace StockControl.Dominio;

public sealed class LineaRecibida
{
    public int IdLinea { get; init; }
    public decimal Cantidad { get; init; }
}

public sealed class ResultadoRecepcion
{
    public int ProductosActualizados { get; set; }
    public int LineasSinProducto { get; set; }
}

public static class ServicioRecepcionOrden
{
    public static ResultadoRecepcion Recibir(
        OrdenCompra orden,
        IEnumerable<LineaRecibida> recibidas,
        IEnumerable<Producto> productos,
        int? idUsuario,
        string nombreUsuario,
        DateTimeOffset fecha)
    {
        if (orden.Recibida)
            throw new ErrorNegocio("La orden ya fue recibida.");
        var cantidades = CantidadesPorLinea(orden, recibidas);
        var porId = productos.ToDictionary(p => p.Id);
        var resultado = new ResultadoRecepcion();

        foreach (var linea in orden.Lineas)
        {
            linea.CantidadRecibida = cantidades.GetValueOrDefault(linea.Id, linea.Cantidad);
            if (!porId.TryGetValue(linea.IdProducto, out var p))
            {
                resultado.LineasSinProducto++;
                continue;
            }
            if (p.ProductoSector || p.EsGenerico) continue;
            p.Cantidad += linea.CantidadRecibida.Value * linea.UnidadesPack;
            p.FechaModificacion = fecha;
            p.UsuarioModificacion = nombreUsuario;
            resultado.ProductosActualizados++;
        }

        orden.FechaRecepcion = fecha;
        orden.IdUsuarioRecepcion = idUsuario;
        orden.NombreUsuarioRecepcion = nombreUsuario;
        return resultado;
    }

    private static Dictionary<int, decimal> CantidadesPorLinea(OrdenCompra orden, IEnumerable<LineaRecibida> recibidas)
    {
        var ids = orden.Lineas.Select(l => l.Id).ToHashSet();
        var cantidades = new Dictionary<int, decimal>();
        foreach (var r in recibidas)
        {
            if (!ids.Contains(r.IdLinea))
                throw new ErrorNegocio("Hay líneas que no son de esta orden.");
            if (r.Cantidad < 0)
                throw new ErrorNegocio("La cantidad recibida no puede ser negativa.");
            cantidades[r.IdLinea] = r.Cantidad;
        }
        return cantidades;
    }
}
