namespace StockControl.Dominio;

public static class ServicioAnularVenta
{
    /// <summary>Caja cerrada: solo se anula si la venta está facturada y la anula dueño o socio (con nota de crédito obligatoria).</summary>
    public static void Anular(
        InformeVenta venta,
        IEnumerable<Producto> productosDelLocal,
        string usuarioModificacion = "",
        bool facturada = false,
        string rol = "")
    {
        if (venta.IdCierre != null && !(facturada && Permisos.PuedeAnularVentaFacturada(rol)))
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
