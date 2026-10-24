namespace StockControl.Dominio;

public static class ServicioOfertas
{
    public static void Validar(Oferta oferta, Producto? producto, IEnumerable<Oferta> otrasDelProducto)
    {
        if (producto == null)
            throw new ErrorNegocio("El producto no existe en este local.");
        if (producto.ProductoSector || producto.EsGenerico)
            throw new ErrorNegocio("Los productos de sector y genéricos no tienen oferta.");
        if (oferta.FechaDesde > oferta.FechaHasta)
            throw new ErrorNegocio("La fecha desde no puede ser posterior a la fecha hasta.");
        ValidarTipo(oferta);
        if (otrasDelProducto.Any(o => o.Id != oferta.Id && SePisan(o, oferta)))
            throw new ErrorNegocio("El producto ya tiene una oferta en esas fechas.");
    }

    public static Oferta? Vigente(IEnumerable<Oferta> ofertas, int idProducto, DateOnly hoy) =>
        ofertas.FirstOrDefault(o => o.IdProducto == idProducto && o.FechaDesde <= hoy && hoy <= o.FechaHasta);

    public static Oferta? Aplicable(PedidoCobro pedido, int idProducto)
    {
        var oferta = Vigente(pedido.Ofertas, idProducto, pedido.Hoy);
        if (oferta == null) return null;
        if (pedido.SinOfertas && oferta.PermiteNoAplicarEnCaja) return null;
        if (pedido.CobrarAlCosto && oferta.AnulaConAlCosto) return null;
        if (pedido.DescuentoPorcentaje > 0 && oferta.AnulaConDescuentoCaja) return null;
        return oferta;
    }

    public static decimal PrecioConOferta(Oferta? oferta, decimal precioLista)
    {
        if (oferta?.Tipo != TipoOferta.Descuento)
            return precioLista;
        return Math.Round(precioLista * (100 - oferta.Porcentaje) / 100m, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>Precio de una unidad con la oferta. Las de precio por cantidad dependen de cuántas se llevan.</summary>
    public static decimal PrecioUnitarioConOferta(Oferta? oferta, decimal precioLista, decimal cantidad) => oferta?.Tipo switch
    {
        TipoOferta.PrecioDesde => cantidad >= oferta.Lleva ? oferta.PrecioUnidad : precioLista,
        TipoOferta.PrecioExacto => cantidad == oferta.Lleva ? oferta.PrecioUnidad : precioLista,
        _ => PrecioConOferta(oferta, precioLista)
    };

    public static decimal CantidadCobrada(Oferta? oferta, decimal cantidad)
    {
        if (oferta?.Tipo != TipoOferta.LlevaPaga)
            return cantidad;
        var grupos = Math.Floor(cantidad / oferta.Lleva);
        return grupos * oferta.Paga + (cantidad - grupos * oferta.Lleva);
    }

    public static string Descripcion(Oferta? oferta) => oferta?.Tipo switch
    {
        TipoOferta.Descuento => $"{oferta.Porcentaje} % off",
        TipoOferta.LlevaPaga => $"{oferta.Lleva}x{oferta.Paga}",
        TipoOferta.PrecioDesde => $"{oferta.Lleva}+ a ${Precio(oferta)}",
        TipoOferta.PrecioExacto => $"{oferta.Lleva} a ${Precio(oferta)}",
        _ => ""
    };

    private static string Precio(Oferta oferta) =>
        oferta.PrecioUnidad.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

    private static void ValidarTipo(Oferta oferta)
    {
        if (oferta.Tipo == TipoOferta.Descuento)
        {
            if (oferta.Porcentaje is < 1 or > 99)
                throw new ErrorNegocio("El descuento va de 1 a 99 %.");
            oferta.Lleva = 0;
            oferta.Paga = 0;
            oferta.PrecioUnidad = 0;
            return;
        }
        if (oferta.Tipo == TipoOferta.LlevaPaga)
        {
            if (oferta.Paga < 1 || oferta.Lleva <= oferta.Paga || oferta.Lleva > 100)
                throw new ErrorNegocio("En llevás N y pagás M, N tiene que ser mayor que M y M al menos 1.");
            oferta.Porcentaje = 0;
            oferta.PrecioUnidad = 0;
            return;
        }
        if (oferta.Tipo is TipoOferta.PrecioDesde or TipoOferta.PrecioExacto)
        {
            if (oferta.Lleva is < 2 or > 1000)
                throw new ErrorNegocio("La cantidad va de 2 a 1000 unidades.");
            if (oferta.PrecioUnidad <= 0)
                throw new ErrorNegocio("El precio por unidad tiene que ser mayor a 0.");
            oferta.Porcentaje = 0;
            oferta.Paga = 0;
            return;
        }
        throw new ErrorNegocio("El tipo de oferta no existe.");
    }

    private static bool SePisan(Oferta a, Oferta b) =>
        a.FechaDesde <= b.FechaHasta && b.FechaDesde <= a.FechaHasta;
}
