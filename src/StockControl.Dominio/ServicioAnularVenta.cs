namespace StockControl.Dominio;

public static class ServicioAnularVenta
{
    public static void Anular(InformeVenta venta, IEnumerable<Producto> productosDelLocal, string usuarioModificacion = "")
    {
        if (venta.IdCierre != null)
            throw new ErrorNegocio("No se puede anular una venta de una caja ya cerrada.");

        var porId = productosDelLocal.ToDictionary(p => p.Id);
        foreach (var d in venta.Detalles)
        {
            if (d.IdProducto is not int id || id == 0)
                continue;
            if (d.Codigo.StartsWith("GENERIC-", StringComparison.OrdinalIgnoreCase))
                continue;
            if (!porId.TryGetValue(id, out var p))
                continue;
            if (p.ProductoSector || p.EsGenerico)
                continue;
            p.Cantidad += d.Cantidad;
            p.FechaModificacion = DateTimeOffset.UtcNow;
            p.UsuarioModificacion = usuarioModificacion;
        }
    }
}
