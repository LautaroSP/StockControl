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

            var linea = ArmarLinea(item, producto, pedido);
            DescontarStock(producto, item.Cantidad, pedido.StockRigido);
            resultado.Lineas.Add(linea);
        }

        ServicioCombos.Aplicar(pedido, resultado.Lineas);
        resultado.Subtotal = resultado.Lineas.Sum(l => l.SubTotal);
        resultado.Total = resultado.Subtotal;
        return resultado;
    }

    private static LineaVenta ArmarLinea(ItemCarrito item, Producto? producto, PedidoCobro pedido)
    {
        decimal? costo = producto?.Costo;
        var precioLibre = producto == null || producto.ProductoSector || producto.EsGenerico;
        var oferta = precioLibre ? null : ServicioOfertas.Aplicable(pedido, producto!.Id);
        var precio = PrecioVenta.PrecioUnitario(
            ServicioOfertas.PrecioUnitarioConOferta(oferta, producto?.Precio ?? 0, item.Cantidad),
            pedido.CobrarAlCosto ? ServicioOfertas.PrecioConOferta(oferta, costo ?? 0) : costo ?? 0,
            pedido.DescuentoPorcentaje,
            pedido.CobrarAlCosto,
            precioLibre,
            item.PrecioUnitario);
        var cobradas = ServicioOfertas.CantidadCobrada(oferta, item.Cantidad);
        var sub = Math.Round(precio * cobradas, 2, MidpointRounding.AwayFromZero);

        return new LineaVenta
        {
            IdProducto = producto?.Id ?? 0,
            Codigo = item.Codigo,
            Nombre = string.IsNullOrWhiteSpace(item.Nombre) ? producto?.Nombre ?? "Producto" : item.Nombre,
            Cantidad = item.Cantidad,
            Precio = precio,
            Costo = costo,
            SubTotal = sub,
            DescripcionOferta = ServicioOfertas.Descripcion(oferta)
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
