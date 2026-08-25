namespace StockControl.Dominio;

public static class ServicioCobro
{
    public static ResultadoCobro Cobrar(PedidoCobro pedido)
    {
        if (pedido.Items.Count == 0)
            throw new ErrorNegocio("El carrito está vacío.");

        var resultado = new ResultadoCobro
        {
            MetodoPago = pedido.MetodoPago,
            Descuento = pedido.DescuentoPorcentaje,
            PrecioCosto = pedido.CobrarAlCosto ? "SI" : "NO"
        };

        foreach (var item in pedido.Items)
        {
            var esGenerico = item.Codigo.StartsWith("GENERIC-", StringComparison.OrdinalIgnoreCase);
            Producto? producto = null;
            if (!esGenerico)
            {
                producto = pedido.ProductosDelLocal.FirstOrDefault(p => p.Id == item.IdProducto);
                if (producto == null)
                    throw new ErrorNegocio($"El producto {item.Nombre} no existe.");
            }

            var linea = ArmarLinea(item, producto, pedido.DescuentoPorcentaje, pedido.CobrarAlCosto);
            DescontarStock(producto, item.Cantidad, pedido.StockRigido);
            resultado.Lineas.Add(linea);
        }

        resultado.Subtotal = resultado.Lineas.Sum(l => l.SubTotal);
        resultado.Total = resultado.Subtotal;
        return resultado;
    }

    private static LineaVenta ArmarLinea(ItemCarrito item, Producto? producto, int descuento, bool alCosto)
    {
        decimal? costo = producto?.Costo;
        var precioLibre = producto == null || producto.ProductoSector || producto.EsGenerico;
        var precio = PrecioVenta.PrecioUnitario(
            producto?.Precio ?? 0,
            producto?.Costo ?? 0,
            descuento,
            alCosto,
            precioLibre,
            item.PrecioUnitario);
        var sub = Math.Round(precio * item.Cantidad, 2, MidpointRounding.AwayFromZero);

        return new LineaVenta
        {
            IdProducto = producto?.Id ?? 0,
            Codigo = item.Codigo,
            Nombre = string.IsNullOrWhiteSpace(item.Nombre) ? producto?.Nombre ?? "Producto" : item.Nombre,
            Cantidad = item.Cantidad,
            Precio = precio,
            Costo = costo,
            SubTotal = sub
        };
    }

    private static void DescontarStock(Producto? producto, decimal cantidad, bool stockRigido)
    {
        if (producto == null || producto.ProductoSector || producto.EsGenerico)
            return;

        if (stockRigido && producto.Cantidad - cantidad < 0)
            throw new ErrorNegocio($"El producto {producto.Nombre} no tiene stock suficiente.");

        producto.Cantidad -= cantidad;
        if (producto.Cantidad < 0)
            producto.Cantidad = 0;
    }
}
