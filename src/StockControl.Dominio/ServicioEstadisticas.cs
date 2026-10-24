namespace StockControl.Dominio;

public class ItemTopProducto
{
    public int? IdProducto { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal Total { get; set; }
    public bool EsSector { get; set; }
}

public class ItemVentaDelDia
{
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal CantidadVentas { get; set; }
    public decimal? StockRestante { get; set; }
    public bool EsSector { get; set; }
    public bool EsGenerico { get; set; }
}

public class ItemMixPago
{
    public string Descripcion { get; set; } = "";
    public decimal Total { get; set; }
    public decimal Porcentaje { get; set; }
    public int CantidadOperaciones { get; set; }
}

public class ItemVentaGrupo
{
    public string NombreGrupo { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal Total { get; set; }
    public decimal Porcentaje { get; set; }
}

public class ItemComparacionLocal
{
    public int IdLocal { get; set; }
    public string NombreLocal { get; set; } = "";
    public decimal TotalVendido { get; set; }
    public int CantidadTickets { get; set; }
    public decimal TicketPromedio { get; set; }
}

public class ItemStockBajo
{
    public int IdProducto { get; set; }
    public int IdLocal { get; set; }
    public string NombreLocal { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal Umbral { get; set; }
}

public class ResumenEstadisticas
{
    public decimal TotalVendido { get; set; }
    public int CantidadTickets { get; set; }
    public decimal TicketPromedio { get; set; }
    public decimal MargenEstimado { get; set; }
    public decimal PorcentajeMargen { get; set; }
    public decimal GananciaDelDia { get; set; }

    public ItemTopProducto? MasVendidoUnidades { get; set; }
    public ItemTopProducto? MasVendidoDinero { get; set; }

    public List<ItemTopProducto> TopPorUnidades { get; set; } = new();
    public List<ItemTopProducto> TopPorDinero { get; set; } = new();
    public List<ItemMixPago> MixMediosPago { get; set; } = new();
    public List<ItemVentaGrupo> VentasPorGrupo { get; set; } = new();
    public List<ItemComparacionLocal> ComparativaSucursales { get; set; } = new();
    public List<ItemStockBajo> StockBajo { get; set; } = new();
    public List<ItemVentaDelDia> VentasDelDia { get; set; } = new();
}

public static class ServicioEstadisticas
{
    public static ResumenEstadisticas Calcular(
        IEnumerable<InformeVenta> ventasPeriodo,
        IEnumerable<InformeVenta> ventasHoy,
        IEnumerable<Producto> productos,
        IEnumerable<GrupoProductos> grupos,
        IEnumerable<Local> locales,
        IDictionary<int, decimal>? umbralesPorLocal = null,
        int topN = 20,
        IDictionary<int, FactoresGananciaLibre>? factoresPorLocal = null,
        IEnumerable<InformeVentaDetalle>? fiadoPeriodo = null,
        IEnumerable<InformeVentaDetalle>? fiadoHoy = null)
    {
        var listaVentas = ventasPeriodo.ToList();
        var listaVentasHoy = ventasHoy.ToList();
        var listaProductos = productos.ToList();
        var listaGrupos = grupos.ToList();
        var listaLocales = locales.ToList();
        var umbrales = umbralesPorLocal ?? new Dictionary<int, decimal>();

        var resumen = new ResumenEstadisticas();

        // 1. Totales generales del período
        resumen.TotalVendido = listaVentas.Sum(v => v.Total);
        resumen.CantidadTickets = listaVentas.Count;
        resumen.TicketPromedio = resumen.CantidadTickets > 0
            ? Math.Round(resumen.TotalVendido / resumen.CantidadTickets, 2, MidpointRounding.AwayFromZero)
            : 0;

        // 2. Margen estimado. Sector y genérico sin costo usan el factor del local.
        var (margen, baseMargen) = SumarGanancia(listaVentas, listaProductos, factoresPorLocal);
        resumen.MargenEstimado = Math.Round(margen, 2, MidpointRounding.AwayFromZero);
        resumen.PorcentajeMargen = baseMargen > 0
            ? Math.Round((margen / baseMargen) * 100m, 2, MidpointRounding.AwayFromZero)
            : 0;
        var (gananciaHoy, _) = SumarGanancia(listaVentasHoy, listaProductos, factoresPorLocal);
        resumen.GananciaDelDia = Math.Round(gananciaHoy, 2, MidpointRounding.AwayFromZero);

        var todosDetallesPeriodo = DetallesConFiado(listaVentas, fiadoPeriodo);

        // Mapa de productos por código para saber si es sector
        var productosPorCodigo = listaProductos
            .GroupBy(p => p.Codigo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // 3. Top productos (ignora genéricos)
        var detallesNoGenericos = todosDetallesPeriodo
            .Where(d => !d.Codigo.StartsWith("GENERIC-", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var gruposTop = detallesNoGenericos
            .GroupBy(d => d.Codigo, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var primerDetalle = g.First();
                productosPorCodigo.TryGetValue(primerDetalle.Codigo, out var prod);
                var esSector = prod?.ProductoSector ?? false;
                var cantidad = esSector ? 0m : g.Sum(x => x.Cantidad);
                var total = g.Sum(x => x.SubTotal);

                return new ItemTopProducto
                {
                    IdProducto = primerDetalle.IdProducto ?? prod?.Id,
                    Codigo = primerDetalle.Codigo,
                    Nombre = !string.IsNullOrWhiteSpace(prod?.Nombre) ? prod.Nombre : primerDetalle.Nombre,
                    Cantidad = cantidad,
                    Total = Math.Round(total, 2, MidpointRounding.AwayFromZero),
                    EsSector = esSector
                };
            })
            .ToList();

        // Top por unidades (excluye sector)
        resumen.TopPorUnidades = gruposTop
            .Where(x => !x.EsSector && x.Cantidad > 0)
            .OrderByDescending(x => x.Cantidad)
            .ThenByDescending(x => x.Total)
            .Take(topN)
            .ToList();

        // Top por dinero (incluye sector)
        resumen.TopPorDinero = gruposTop
            .Where(x => x.Total > 0)
            .OrderByDescending(x => x.Total)
            .ThenByDescending(x => x.Cantidad)
            .Take(topN)
            .ToList();

        resumen.MasVendidoUnidades = resumen.TopPorUnidades.FirstOrDefault();
        resumen.MasVendidoDinero = resumen.TopPorDinero.FirstOrDefault();

        // 4. Mix de medios de pago
        var pagosAcumulados = new Dictionary<string, (decimal total, int cant)>(StringComparer.OrdinalIgnoreCase);
        foreach (var v in listaVentas)
        {
            if (v.Pagos != null && v.Pagos.Count > 0)
            {
                foreach (var p in v.Pagos)
                {
                    var desc = string.IsNullOrWhiteSpace(p.DescripcionMetodoPago) ? "Otros" : p.DescripcionMetodoPago.Trim();
                    if (!pagosAcumulados.TryGetValue(desc, out var actual))
                        actual = (0m, 0);
                    pagosAcumulados[desc] = (actual.total + p.Importe, actual.cant + 1);
                }
            }
            else
            {
                var desc = string.IsNullOrWhiteSpace(v.MetodoPago) ? "Sin especificar" : v.MetodoPago.Trim();
                if (!pagosAcumulados.TryGetValue(desc, out var actual))
                    actual = (0m, 0);
                pagosAcumulados[desc] = (actual.total + v.Total, actual.cant + 1);
            }
        }

        var totalMix = pagosAcumulados.Values.Sum(x => x.total);
        resumen.MixMediosPago = pagosAcumulados
            .Select(kv => new ItemMixPago
            {
                Descripcion = kv.Key,
                Total = Math.Round(kv.Value.total, 2, MidpointRounding.AwayFromZero),
                CantidadOperaciones = kv.Value.cant,
                Porcentaje = totalMix > 0
                    ? Math.Round((kv.Value.total / totalMix) * 100m, 2, MidpointRounding.AwayFromZero)
                    : 0
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        // 5. Ventas por grupo de precio
        var mapaGrupos = listaGrupos.ToDictionary(g => g.IdGrupoProducto, g => g.NombreGrupo);
        var ventasPorGrupoAcum = new Dictionary<string, (decimal cant, decimal total)>(StringComparer.OrdinalIgnoreCase);

        foreach (var d in todosDetallesPeriodo)
        {
            string nombreGrupo = "Sin grupo";
            if (productosPorCodigo.TryGetValue(d.Codigo, out var prod) && prod.IdGrupoProducto > 0)
            {
                if (mapaGrupos.TryGetValue(prod.IdGrupoProducto, out var ng))
                    nombreGrupo = ng;
            }

            if (!ventasPorGrupoAcum.TryGetValue(nombreGrupo, out var actual))
                actual = (0m, 0m);

            ventasPorGrupoAcum[nombreGrupo] = (actual.cant + d.Cantidad, actual.total + d.SubTotal);
        }

        var totalGrupos = ventasPorGrupoAcum.Values.Sum(x => x.total);
        resumen.VentasPorGrupo = ventasPorGrupoAcum
            .Select(kv => new ItemVentaGrupo
            {
                NombreGrupo = kv.Key,
                Cantidad = kv.Value.cant,
                Total = Math.Round(kv.Value.total, 2, MidpointRounding.AwayFromZero),
                Porcentaje = totalGrupos > 0
                    ? Math.Round((kv.Value.total / totalGrupos) * 100m, 2, MidpointRounding.AwayFromZero)
                    : 0
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        // 6. Comparativa de sucursales
        var mapaNombresLocales = listaLocales.ToDictionary(l => l.IdLocal, l => l.Nombre);
        resumen.ComparativaSucursales = listaVentas
            .GroupBy(v => v.IdLocal)
            .Select(g =>
            {
                var totalVendido = g.Sum(v => v.Total);
                var tickets = g.Count();
                mapaNombresLocales.TryGetValue(g.Key, out var nombreLoc);
                return new ItemComparacionLocal
                {
                    IdLocal = g.Key,
                    NombreLocal = !string.IsNullOrWhiteSpace(nombreLoc) ? nombreLoc : $"Local {g.Key}",
                    TotalVendido = Math.Round(totalVendido, 2, MidpointRounding.AwayFromZero),
                    CantidadTickets = tickets,
                    TicketPromedio = tickets > 0
                        ? Math.Round(totalVendido / tickets, 2, MidpointRounding.AwayFromZero)
                        : 0
                };
            })
            .OrderByDescending(x => x.TotalVendido)
            .ToList();

        // 7. Stock bajo
        resumen.StockBajo = listaProductos
            .Where(p => !p.ProductoSector && !p.EsGenerico)
            .Where(p =>
            {
                var umbral = umbrales.TryGetValue(p.IdLocal, out var u) ? u : 5m;
                return p.Cantidad <= umbral;
            })
            .Select(p =>
            {
                mapaNombresLocales.TryGetValue(p.IdLocal, out var nombreLoc);
                var umbral = umbrales.TryGetValue(p.IdLocal, out var u) ? u : 5m;
                return new ItemStockBajo
                {
                    IdProducto = p.Id,
                    IdLocal = p.IdLocal,
                    NombreLocal = !string.IsNullOrWhiteSpace(nombreLoc) ? nombreLoc : $"Local {p.IdLocal}",
                    Codigo = p.Codigo,
                    Nombre = p.Nombre,
                    Cantidad = p.Cantidad,
                    Umbral = umbral
                };
            })
            .OrderBy(x => x.Cantidad)
            .ToList();

        // 8. Ventas del día (consolida todas las cajas, ordenado por cantidad descendente)
        var todosDetallesHoy = DetallesConFiado(listaVentasHoy, fiadoHoy);
        resumen.VentasDelDia = todosDetallesHoy
            .GroupBy(d => d.Codigo, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var primerDetalle = g.First();
                var esGenerico = primerDetalle.Codigo.StartsWith("GENERIC-", StringComparison.OrdinalIgnoreCase);
                productosPorCodigo.TryGetValue(primerDetalle.Codigo, out var prod);
                var esSector = prod?.ProductoSector ?? false;

                decimal? stockRestante = null;
                if (!esGenerico && !esSector)
                {
                    var prodsConCodigo = listaProductos
                        .Where(p => string.Equals(p.Codigo, primerDetalle.Codigo, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (prodsConCodigo.Count > 0)
                        stockRestante = prodsConCodigo.Sum(p => p.Cantidad);
                    else if (prod != null)
                        stockRestante = prod.Cantidad;
                }

                return new ItemVentaDelDia
                {
                    Codigo = primerDetalle.Codigo,
                    Nombre = !string.IsNullOrWhiteSpace(prod?.Nombre) ? prod.Nombre : primerDetalle.Nombre,
                    CantidadVentas = g.Sum(x => x.Cantidad),
                    StockRestante = stockRestante,
                    EsSector = esSector,
                    EsGenerico = esGenerico
                };
            })
            .OrderByDescending(x => x.CantidadVentas)
            .ThenBy(x => x.Nombre)
            .ToList();

        return resumen;
    }

    /// <summary>Unidades vendidas: sin la línea de pago de cuenta (no es mercadería) y con lo fiado (salió del stock).</summary>
    static List<InformeVentaDetalle> DetallesConFiado(IEnumerable<InformeVenta> ventas, IEnumerable<InformeVentaDetalle>? fiado) =>
        ventas.SelectMany(v => v.Detalles)
            .Where(d => !ServicioCuentas.EsPagoDeCuenta(d.Codigo))
            .Concat(fiado ?? [])
            .ToList();

    /// <summary>
    /// Con costo, la ganancia es precio − costo. Sector o genérico sin costo: precio − precio / factor del local.
    /// Factor 1 o menor no estima: esa línea queda afuera.
    /// </summary>
    public static decimal? GananciaDeDetalle(
        decimal subTotal,
        decimal? costo,
        decimal cantidad,
        bool esSector,
        bool esGenerico,
        decimal factorSector,
        decimal factorGenerico)
    {
        if (costo is > 0)
            return subTotal - costo.Value * cantidad;
        if (!esSector && !esGenerico)
            return null;
        var factor = esSector ? factorSector : factorGenerico;
        if (factor <= 1 || subTotal == 0)
            return null;
        return subTotal - (subTotal / factor);
    }

    static (decimal ganancia, decimal baseVenta) SumarGanancia(
        IEnumerable<InformeVenta> ventas,
        IReadOnlyList<Producto> productos,
        IDictionary<int, FactoresGananciaLibre>? factoresPorLocal)
    {
        decimal ganancia = 0;
        decimal baseVenta = 0;
        foreach (var venta in ventas)
        {
            var factores = factoresPorLocal != null && factoresPorLocal.TryGetValue(venta.IdLocal, out var delLocal)
                ? delLocal
                : new FactoresGananciaLibre(1, 1);
            var factorGenerico = factores.Generico > 1 ? factores.Generico : factores.Comun;
            foreach (var detalle in venta.Detalles)
            {
                var esGenerico = detalle.Codigo.StartsWith("GENERIC-", StringComparison.OrdinalIgnoreCase);
                var producto = productos.FirstOrDefault(p =>
                    p.IdLocal == venta.IdLocal
                    && string.Equals(p.Codigo, detalle.Codigo, StringComparison.OrdinalIgnoreCase));
                var estimada = GananciaDeDetalle(
                    detalle.SubTotal,
                    detalle.Costo,
                    detalle.Cantidad,
                    producto?.ProductoSector == true,
                    esGenerico,
                    factores.Sector,
                    factorGenerico);
                if (estimada == null)
                    continue;
                ganancia += estimada.Value;
                baseVenta += detalle.SubTotal;
            }
        }
        return (ganancia, baseVenta);
    }
}

public readonly record struct FactoresGananciaLibre(decimal Sector, decimal Generico, decimal Comun = 1);
