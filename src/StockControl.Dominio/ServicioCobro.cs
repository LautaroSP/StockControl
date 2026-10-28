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
            DescontarStock(producto, item.Cantidad, pedido.StockRigido, pedido.Reservado);
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
        var precioOferta = ServicioOfertas.PrecioUnitarioConOferta(oferta, producto?.Precio ?? 0, item.Cantidad);
        var costoOferta = pedido.CobrarAlCosto ? ServicioOfertas.PrecioConOferta(oferta, costo ?? 0) : costo ?? 0;
        var precio = PrecioVenta.PrecioUnitario(
            precioOferta,
            costoOferta,
            pedido.DescuentoPorcentaje,
            pedido.CobrarAlCosto,
            precioLibre,
            item.PrecioUnitario);
        var cobradas = ServicioOfertas.CantidadCobrada(oferta, item.Cantidad);
        var sub = Redondear(precio * cobradas);
        var precioBase = PrecioBase(producto, precioLibre, pedido.CobrarAlCosto, item.PrecioUnitario);
        var conOferta = precioLibre || pedido.CobrarAlCosto ? sub : Redondear(precioOferta * cobradas);

        return new LineaVenta
        {
            IdProducto = producto?.Id ?? 0,
            Codigo = item.Codigo,
            Nombre = string.IsNullOrWhiteSpace(item.Nombre) ? producto?.Nombre ?? "Producto" : item.Nombre,
            Cantidad = item.Cantidad,
            Precio = precio,
            Costo = costo,
            SubTotal = sub,
            DescripcionOferta = ServicioOfertas.Descripcion(oferta),
            PrecioBase = precioBase,
            AhorroOferta = Redondear(precioBase * item.Cantidad) - conOferta,
            AhorroDescuento = conOferta - sub
        };
    }

    /// <summary>Precio unitario antes de ofertas y descuento: lista, costo si se cobra al costo, o el de caja si es libre.</summary>
    private static decimal PrecioBase(Producto? producto, bool precioLibre, bool alCosto, decimal precioEnCaja)
    {
        if (precioLibre) return Redondear(precioEnCaja);
        return Redondear(alCosto ? producto!.Costo : producto!.Precio);
    }

    private static decimal Redondear(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    private static void DescontarStock(Producto? producto, decimal cantidad, bool stockRigido, IReadOnlyDictionary<int, decimal> reservado)
    {
        if (producto == null || producto.ProductoSector || producto.EsGenerico)
            return;

        if (stockRigido && ServicioPedidosTienda.Disponible(producto, reservado) - cantidad < 0)
            throw new ErrorNegocio($"El producto {producto.Nombre} no tiene stock suficiente.");

        producto.Cantidad -= cantidad;
        if (producto.Cantidad < 0)
            producto.Cantidad = 0;
    }
}
