namespace StockControl.Dominio;

public static class ServicioCombos
{
    public static void Validar(OfertaCombo combo, IReadOnlyCollection<Producto> productosDelLocal)
    {
        if (string.IsNullOrWhiteSpace(combo.Nombre))
            throw new ErrorNegocio("El combo necesita un nombre.");
        if (combo.PrecioTotal <= 0)
            throw new ErrorNegocio("El precio del combo tiene que ser mayor a 0.");
        if (combo.FechaDesde > combo.FechaHasta)
            throw new ErrorNegocio("La fecha desde no puede ser posterior a la fecha hasta.");
        if (combo.Lineas.Count < 2)
            throw new ErrorNegocio("Un combo lleva al menos dos productos.");
        if (combo.Lineas.Select(l => l.IdProducto).Distinct().Count() != combo.Lineas.Count)
            throw new ErrorNegocio("Un producto no puede repetirse en el combo.");
        foreach (var linea in combo.Lineas)
            ValidarLinea(linea, productosDelLocal);
    }

    public static bool Vigente(OfertaCombo combo, DateOnly hoy) =>
        combo.FechaDesde <= hoy && hoy <= combo.FechaHasta;

    /// <summary>Baja el subtotal de las líneas por cada combo completo, solo si le conviene al cliente.</summary>
    public static void Aplicar(PedidoCobro pedido, List<LineaVenta> lineas)
    {
        if (pedido.SinOfertas || pedido.CobrarAlCosto || pedido.DescuentoPorcentaje > 0)
            return;
        foreach (var combo in pedido.Combos.Where(c => Vigente(c, pedido.Hoy)))
            AplicarCombo(combo, lineas);
    }

    private static void ValidarLinea(LineaOfertaCombo linea, IReadOnlyCollection<Producto> productos)
    {
        var producto = productos.FirstOrDefault(p => p.Id == linea.IdProducto)
            ?? throw new ErrorNegocio("Un producto del combo no existe en este local.");
        if (producto.ProductoSector || producto.EsGenerico)
            throw new ErrorNegocio("Los productos de sector y genéricos no entran en un combo.");
        if (linea.Cantidad is < 1 or > 100)
            throw new ErrorNegocio("La cantidad de cada producto va de 1 a 100.");
    }

    private static void AplicarCombo(OfertaCombo combo, List<LineaVenta> lineas)
    {
        var completos = Completos(combo, lineas);
        if (completos == 0) return;
        var tomadas = Tomar(combo, lineas, completos);
        var valor = tomadas.Sum(t => t.Valor);
        var descuento = valor - completos * combo.PrecioTotal;
        if (descuento <= 0) return;
        Repartir(tomadas, descuento, valor, combo.Nombre);
    }

    private static int Completos(OfertaCombo combo, List<LineaVenta> lineas) =>
        combo.Lineas.Min(c => (int)Math.Floor(lineas.Where(l => l.IdProducto == c.IdProducto).Sum(l => l.Cantidad) / c.Cantidad));

    private static List<(LineaVenta Linea, decimal Valor)> Tomar(OfertaCombo combo, List<LineaVenta> lineas, int completos)
    {
        var tomadas = new List<(LineaVenta, decimal)>();
        foreach (var c in combo.Lineas)
        {
            var faltan = (decimal)c.Cantidad * completos;
            foreach (var l in lineas.Where(l => l.IdProducto == c.IdProducto && l.Cantidad > 0))
            {
                if (faltan <= 0) break;
                var unidades = Math.Min(faltan, l.Cantidad);
                tomadas.Add((l, unidades * l.SubTotal / l.Cantidad));
                faltan -= unidades;
            }
        }
        return tomadas;
    }

    private static void Repartir(List<(LineaVenta Linea, decimal Valor)> tomadas, decimal descuento, decimal valor, string nombre)
    {
        var restante = descuento;
        for (var i = 0; i < tomadas.Count; i++)
        {
            var parte = i == tomadas.Count - 1
                ? restante
                : Math.Round(descuento * tomadas[i].Valor / valor, 2, MidpointRounding.AwayFromZero);
            var linea = tomadas[i].Linea;
            var nuevo = Math.Max(0, linea.SubTotal - parte);
            linea.AhorroCombo += linea.SubTotal - nuevo;
            linea.SubTotal = nuevo;
            linea.NombreCombo = nombre;
            restante -= parte;
        }
    }
}
