namespace StockControl.Domain
{
    public class ResultadoCierre
    {
        public List<Caja> Cajas { get; set; } = new();
        public List<CajaProducto> Productos { get; set; } = new();
    }

    public static class CierreCaja
    {
        public static ResultadoCierre Calcular(
            IReadOnlyList<InformeVenta> informes,
            IReadOnlyDictionary<int, List<InformeVentaDetalle>> detalles,
            IReadOnlyList<PagoProveedor> pagos,
            IReadOnlyDictionary<string, Producto> productosPorCodigo,
            IReadOnlyDictionary<string, decimal> vendidoDespuesPorCodigo,
            IReadOnlyDictionary<string, decimal> agregadoDespuesPorCodigo,
            DateTime fecha,
            string idCierre,
            bool restarPagosProveedor)
        {
            var gananciaPorMetodo = new Dictionary<string, decimal>();
            var genericosPorMetodo = new Dictionary<string, decimal>();
            var totalPorMetodo = new Dictionary<string, decimal>();
            var cantidadPorMetodo = new Dictionary<string, int>();
            var pagoPorMetodo = new Dictionary<string, decimal>();

            foreach (var grupo in Agrupar(informes))
            {
                var (ganancia, genericos) = TotalesTicket(grupo, detalles, productosPorCodigo);
                decimal totalMedios = grupo.Sum(g => g.Total);
                foreach (var informe in grupo)
                {
                    decimal porcentaje = totalMedios == 0
                        ? 1m / grupo.Count
                        : informe.Total / totalMedios;
                    string metodo = informe.MetodoPago ?? string.Empty;
                    Sumar(gananciaPorMetodo, metodo, ganancia * porcentaje);
                    Sumar(genericosPorMetodo, metodo, genericos * porcentaje);
                    Sumar(totalPorMetodo, metodo, informe.Total);
                    cantidadPorMetodo.TryGetValue(metodo, out int cantidad);
                    cantidadPorMetodo[metodo] = cantidad + 1;
                }
            }

            if (restarPagosProveedor)
            {
                foreach (var pago in pagos)
                {
                    string metodo = pago.MetodoPago ?? string.Empty;
                    Sumar(gananciaPorMetodo, metodo, -pago.Monto);
                    Sumar(pagoPorMetodo, metodo, pago.Monto);
                    if (!totalPorMetodo.ContainsKey(metodo))
                        totalPorMetodo[metodo] = 0;
                }
            }

            var orden = new List<string>();
            foreach (var informe in informes)
            {
                string metodo = informe.MetodoPago ?? string.Empty;
                if (!orden.Contains(metodo))
                    orden.Add(metodo);
            }
            foreach (var metodo in totalPorMetodo.Keys)
            {
                if (!orden.Contains(metodo))
                    orden.Add(metodo);
            }

            var cajas = new List<Caja>();
            foreach (var metodo in orden)
            {
                cajas.Add(new Caja
                {
                    Fecha = fecha,
                    MetodoPago = metodo,
                    Total = Dinero(totalPorMetodo.GetValueOrDefault(metodo)),
                    CantidadVentas = cantidadPorMetodo.GetValueOrDefault(metodo),
                    Ganancia = Dinero(gananciaPorMetodo.GetValueOrDefault(metodo)),
                    GenericosOSector = Dinero(genericosPorMetodo.GetValueOrDefault(metodo)),
                    PagoProveedor = Dinero(pagoPorMetodo.GetValueOrDefault(metodo)),
                    IdCierre = idCierre
                });
            }

            if (cajas.Count > 0)
            {
                cajas.Add(new Caja
                {
                    Fecha = fecha,
                    MetodoPago = "Total",
                    Total = cajas.Sum(c => c.Total),
                    CantidadVentas = cajas.Sum(c => c.CantidadVentas),
                    Ganancia = cajas.Sum(c => c.Ganancia),
                    GenericosOSector = cajas.Sum(c => c.GenericosOSector),
                    PagoProveedor = cajas.Sum(c => c.PagoProveedor),
                    IdCierre = idCierre
                });
            }

            return new ResultadoCierre
            {
                Cajas = cajas,
                Productos = ArmarProductos(detalles, productosPorCodigo, vendidoDespuesPorCodigo, agregadoDespuesPorCodigo, idCierre)
            };
        }

        // Stock final = stock de ahora + ventas posteriores − entradas posteriores.
        // Stock inicial = stock final + lo vendido ese día (incluye lo que entró ese día).
        private static List<CajaProducto> ArmarProductos(
            IReadOnlyDictionary<int, List<InformeVentaDetalle>> detalles,
            IReadOnlyDictionary<string, Producto> productosPorCodigo,
            IReadOnlyDictionary<string, decimal> vendidoDespuesPorCodigo,
            IReadOnlyDictionary<string, decimal> agregadoDespuesPorCodigo,
            string idCierre)
        {
            var acumulados = new Dictionary<string, AcumuladoProducto>(StringComparer.OrdinalIgnoreCase);
            foreach (var lineas in detalles.Values)
            {
                foreach (var linea in lineas)
                {
                    if (string.IsNullOrWhiteSpace(linea.Codigo))
                        continue;

                    if (!acumulados.TryGetValue(linea.Codigo, out var acumulado))
                    {
                        acumulado = new AcumuladoProducto
                        {
                            Codigo = linea.Codigo,
                            Nombre = linea.Nombre ?? string.Empty
                        };
                        acumulados[linea.Codigo] = acumulado;
                    }

                    productosPorCodigo.TryGetValue(linea.Codigo, out var producto);
                    bool generico = EsGenerico(linea);
                    bool sector = !generico && EsSector(linea, producto);
                    acumulado.Cantidad += linea.Cantidad;
                    if (!string.IsNullOrWhiteSpace(producto?.Nombre))
                        acumulado.Nombre = producto.Nombre;

                    if (generico)
                        acumulado.Generico = true;
                    else if (sector)
                        acumulado.Sector = true;
                    else
                    {
                        acumulado.Normal = true;
                        acumulado.Ganancia += (linea.Precio - CostoDe(linea, producto)) * linea.Cantidad;
                    }
                }
            }

            var productos = new List<CajaProducto>();
            foreach (var acumulado in acumulados.Values.OrderBy(a => a.Nombre))
            {
                bool sinGanancia = acumulado.Generico || (acumulado.Sector && !acumulado.Normal);
                var fila = new CajaProducto
                {
                    IdCierre = idCierre,
                    Codigo = acumulado.Codigo,
                    Nombre = acumulado.Nombre,
                    CantidadVendida = acumulado.Cantidad,
                    EsGenerico = acumulado.Generico && !acumulado.Normal ? 1 : 0,
                    EsSector = acumulado.Sector && !acumulado.Normal ? 1 : 0,
                    Ganancia = sinGanancia ? null : Dinero(acumulado.Ganancia)
                };

                if (!sinGanancia && productosPorCodigo.TryGetValue(acumulado.Codigo, out var producto))
                {
                    decimal vendidoDespues = vendidoDespuesPorCodigo.GetValueOrDefault(acumulado.Codigo);
                    decimal agregadoDespues = agregadoDespuesPorCodigo.GetValueOrDefault(acumulado.Codigo);
                    decimal stockFinal = producto.Cantidad + vendidoDespues - agregadoDespues;
                    fila.StockFinal = stockFinal;
                    fila.StockInicial = stockFinal + acumulado.Cantidad;
                }

                productos.Add(fila);
            }

            return productos;
        }

        private static List<List<InformeVenta>> Agrupar(IReadOnlyList<InformeVenta> informes)
        {
            var grupos = new List<List<InformeVenta>>();
            var ordenados = informes.OrderBy(i => i.IdInformeVenta).ToList();

            foreach (var grupo in ordenados.Where(i => i.IdGrupo > 0).GroupBy(i => i.IdGrupo))
                grupos.Add(grupo.ToList());

            var resto = ordenados.Where(i => i.IdGrupo == 0).ToList();
            int indice = 0;
            while (indice < resto.Count)
            {
                var actual = resto[indice];
                if (actual.MultipleMetodoDePago == 1 && actual.DetalleAdjunto == 1)
                {
                    var grupo = new List<InformeVenta> { actual };
                    int siguiente = indice + 1;
                    while (siguiente < resto.Count
                        && resto[siguiente].MultipleMetodoDePago == 1
                        && resto[siguiente].DetalleAdjunto == 0)
                    {
                        grupo.Add(resto[siguiente]);
                        siguiente++;
                    }
                    grupos.Add(grupo);
                    indice = siguiente;
                }
                else
                {
                    grupos.Add(new List<InformeVenta> { actual });
                    indice++;
                }
            }

            return grupos;
        }

        private static (decimal ganancia, decimal genericos) TotalesTicket(
            IReadOnlyList<InformeVenta> grupo,
            IReadOnlyDictionary<int, List<InformeVentaDetalle>> detalles,
            IReadOnlyDictionary<string, Producto> productosPorCodigo)
        {
            decimal ganancia = 0;
            decimal genericos = 0;
            foreach (var informe in grupo)
            {
                if (!detalles.TryGetValue(informe.IdInformeVenta, out var lineas))
                    continue;

                foreach (var linea in lineas)
                {
                    productosPorCodigo.TryGetValue(linea.Codigo ?? string.Empty, out var producto);
                    if (EsGenerico(linea) || EsSector(linea, producto))
                        genericos += linea.Precio * linea.Cantidad;
                    else
                        ganancia += (linea.Precio - CostoDe(linea, producto)) * linea.Cantidad;
                }
            }

            return (ganancia, genericos);
        }

        private static bool EsGenerico(InformeVentaDetalle linea)
        {
            if (linea.EsGenerico == 1)
                return true;
            if (linea.ClasificacionRegistrada == 1)
                return false;
            return linea.Codigo != null && linea.Codigo.Contains("GENERIC", StringComparison.OrdinalIgnoreCase);
        }

        private static bool EsSector(InformeVentaDetalle linea, Producto? producto)
        {
            if (linea.ClasificacionRegistrada == 1)
                return linea.EsSector == 1;
            return producto != null && producto.ProductoSector == 1;
        }

        private static decimal CostoDe(InformeVentaDetalle linea, Producto? producto)
        {
            if (linea.ClasificacionRegistrada == 1)
                return linea.CostoVenta;
            return producto?.Costo ?? 0m;
        }

        private static void Sumar(Dictionary<string, decimal> mapa, string clave, decimal valor)
        {
            mapa.TryGetValue(clave, out decimal actual);
            mapa[clave] = actual + valor;
        }

        private static decimal Dinero(decimal valor) =>
            Math.Round(valor, 2, MidpointRounding.AwayFromZero);

        private sealed class AcumuladoProducto
        {
            public string Codigo = string.Empty;
            public string Nombre = string.Empty;
            public decimal Cantidad;
            public decimal Ganancia;
            public bool Generico;
            public bool Sector;
            public bool Normal;
        }
    }
}
