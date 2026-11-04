namespace StockControl.Dominio;

/// <summary>Entregar un pedido de la tienda lo convierte en venta del local con el precio congelado. La gestión no es venta.</summary>
public static class ServicioEntregaPedidoTienda
{
    public static ResultadoCobro Entregar(PedidoTienda pedido, IReadOnlyCollection<Producto> productos, DateTimeOffset ahora)
    {
        ServicioPagoPedidoTienda.ExigirSinPagoPendiente(pedido);
        ServicioPedidosTienda.CambiarEstado(pedido, ServicioPedidosTienda.Entregado, ahora);
        pedido.Entregado = ahora;

        var resultado = new ResultadoCobro { Subtotal = pedido.Subtotal, Total = pedido.Subtotal };
        foreach (var linea in pedido.Lineas)
        {
            var producto = productos.FirstOrDefault(p => p.Id == linea.IdProducto && p.IdLocal == pedido.IdLocal);
            DescontarStock(producto, linea.Cantidad);
            resultado.Lineas.Add(ALineaVenta(linea, producto));
        }
        if (pedido.Envio > 0)
        {
            resultado.Lineas.Add(new LineaVenta
            {
                Codigo = "ENVIO",
                Nombre = "Envío",
                Cantidad = 1,
                Precio = pedido.Envio,
                SubTotal = pedido.Envio,
                PrecioBase = pedido.Envio
            });
            resultado.Subtotal += pedido.Envio;
            resultado.Total += pedido.Envio;
        }
        return resultado;
    }

    /// <summary>La reserva ya apartó las unidades: no se vuelve a exigir stock. Si alguien lo bajó a mano, queda en 0.</summary>
    private static void DescontarStock(Producto? producto, decimal cantidad)
    {
        if (producto == null || producto.ProductoSector || producto.EsGenerico)
            return;
        producto.Cantidad = Math.Max(0, producto.Cantidad - cantidad);
    }

    private static LineaVenta ALineaVenta(LineaPedidoTienda linea, Producto? producto) => new()
    {
        IdProducto = producto?.Id ?? 0,
        Codigo = linea.Codigo,
        Nombre = linea.Nombre,
        Cantidad = linea.Cantidad,
        Precio = linea.PrecioUnitario,
        Costo = producto?.Costo,
        SubTotal = linea.SubTotal,
        PrecioBase = linea.PrecioUnitario
    };
}
