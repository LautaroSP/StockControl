namespace StockControl.Dominio;

public static class ServicioCuentas
{
    public const string PrefijoCodigo = "CUENTA-";

    public static Cliente NuevoCliente(int idLocal, string nombre, string? telefono)
    {
        var limpio = (nombre ?? "").Trim();
        var tel = (telefono ?? "").Trim();
        if (limpio.Length == 0)
            throw new ErrorNegocio("Poné el nombre del cliente.");
        if (limpio.Length > 80)
            throw new ErrorNegocio("El nombre admite hasta 80 caracteres.");
        if (tel.Length > 30)
            throw new ErrorNegocio("El teléfono admite hasta 30 caracteres.");
        return new Cliente { IdLocal = idLocal, Nombre = limpio, Telefono = tel };
    }

    public static CuentaPendiente Fiar(
        int idLocal,
        int idCliente,
        ResultadoCobro resultado,
        int nroCaja,
        int? idUsuario,
        string nombreUsuario,
        decimal? gananciaEstimada)
    {
        if (resultado.Total <= 0)
            throw new ErrorNegocio("No hay importe para dejar a cuenta.");

        return new CuentaPendiente
        {
            IdLocal = idLocal,
            IdCliente = idCliente,
            NroCaja = nroCaja,
            IdUsuario = idUsuario,
            NombreUsuario = nombreUsuario,
            Importe = resultado.Total,
            Saldo = resultado.Total,
            GananciaEstimada = gananciaEstimada,
            Lineas = resultado.Lineas.Select(l => new LineaCuenta
            {
                IdLocal = idLocal,
                IdProducto = l.IdProducto == 0 ? null : l.IdProducto,
                Codigo = l.Codigo,
                Nombre = l.Nombre,
                Cantidad = l.Cantidad,
                Precio = l.Precio,
                Costo = l.Costo,
                SubTotal = l.SubTotal,
                DescripcionOferta = l.DescripcionOferta,
                PrecioBase = l.PrecioBase,
                AhorroOferta = l.AhorroOferta,
                AhorroDescuento = l.AhorroDescuento,
                AhorroCombo = l.AhorroCombo,
                NombreCombo = l.NombreCombo
            }).ToList()
        };
    }

    /// <summary>Ganancia del ticket fiado con la misma regla que Estadísticas. Null si ninguna línea se puede estimar.</summary>
    public static decimal? GananciaEstimada(IEnumerable<LineaVenta> lineas, IReadOnlyList<Producto> productos, FactoresGananciaLibre factores)
    {
        var factorGenerico = factores.Generico > 1 ? factores.Generico : factores.Comun;
        var estimadas = lineas
            .Select(l => ServicioEstadisticas.GananciaDeDetalle(
                l.SubTotal,
                l.Costo,
                l.Cantidad,
                productos.FirstOrDefault(p => p.Id == l.IdProducto)?.ProductoSector == true,
                l.Codigo.StartsWith("GENERIC-", StringComparison.OrdinalIgnoreCase),
                factores.Sector,
                factorGenerico))
            .Where(g => g != null)
            .ToList();
        return estimadas.Count == 0 ? null : estimadas.Sum(g => g!.Value);
    }

    /// <summary>Baja el saldo y devuelve la línea de la venta del pago, con la ganancia proporcional a lo pagado.</summary>
    public static (PagoCuenta pago, LineaVenta linea) Pagar(CuentaPendiente cuenta, string nombreCliente, decimal importe, int? idUsuario)
    {
        if (cuenta.Estado != EstadoCuenta.Abierta)
            throw new ErrorNegocio("La cuenta no está abierta.");
        var monto = Math.Round(importe, 2, MidpointRounding.AwayFromZero);
        if (monto <= 0)
            throw new ErrorNegocio("El pago tiene que ser mayor a cero.");
        if (monto > cuenta.Saldo)
            throw new ErrorNegocio($"No se puede pagar más que el saldo ({cuenta.Saldo:0.00}).");

        cuenta.Saldo -= monto;
        if (cuenta.Saldo == 0)
            cuenta.Estado = EstadoCuenta.Pagada;
        var pago = new PagoCuenta { IdLocal = cuenta.IdLocal, Importe = monto, IdUsuario = idUsuario };
        cuenta.Pagos.Add(pago);

        decimal? ganancia = cuenta.GananciaEstimada is decimal g && cuenta.Importe > 0
            ? Math.Round(g * monto / cuenta.Importe, 2, MidpointRounding.AwayFromZero)
            : null;
        return (pago, new LineaVenta
        {
            Codigo = $"{PrefijoCodigo}{cuenta.Id}",
            Nombre = $"Pago cuenta de {nombreCliente}",
            Cantidad = 1,
            Precio = monto,
            Costo = ganancia == null ? null : monto - ganancia,
            SubTotal = monto
        });
    }

    public static void DevolverPago(CuentaPendiente cuenta, PagoCuenta pago)
    {
        cuenta.Saldo += pago.Importe;
        if (cuenta.Estado == EstadoCuenta.Pagada)
            cuenta.Estado = EstadoCuenta.Abierta;
        cuenta.Pagos.Remove(pago);
    }

    public static void Incobrable(CuentaPendiente cuenta, string usuario, DateTimeOffset fecha)
    {
        if (cuenta.Estado != EstadoCuenta.Abierta)
            throw new ErrorNegocio("Solo una cuenta abierta se puede anular como incobrable.");
        cuenta.Estado = EstadoCuenta.Incobrable;
        cuenta.FechaIncobrable = fecha;
        cuenta.UsuarioIncobrable = usuario;
    }

    public static List<DeudaCliente> DeudaPorCliente(IEnumerable<CuentaPendiente> cuentas, IReadOnlyDictionary<int, string> nombres) =>
        cuentas
            .Where(c => c.Estado == EstadoCuenta.Abierta && c.Saldo > 0)
            .GroupBy(c => c.IdCliente)
            .Select(g => new DeudaCliente
            {
                IdCliente = g.Key,
                Nombre = nombres.TryGetValue(g.Key, out var n) ? n : $"Cliente {g.Key}",
                Saldo = g.Sum(c => c.Saldo),
                Cuentas = g.Count()
            })
            .OrderByDescending(d => d.Saldo)
            .ThenBy(d => d.Nombre)
            .ToList();

    /// <summary>Unidades fiadas como detalle sin plata: cuentan en top y ventas del día, no en el dinero.</summary>
    public static List<InformeVentaDetalle> UnidadesFiadas(IEnumerable<CuentaPendiente> cuentas) =>
        cuentas
            .SelectMany(c => c.Lineas)
            .Select(l => new InformeVentaDetalle
            {
                IdLocal = l.IdLocal,
                IdProducto = l.IdProducto,
                Codigo = l.Codigo,
                Nombre = l.Nombre,
                Cantidad = l.Cantidad,
                Precio = 0,
                SubTotal = 0
            })
            .ToList();

    public static bool EsPagoDeCuenta(string codigo) =>
        codigo.StartsWith(PrefijoCodigo, StringComparison.OrdinalIgnoreCase);
}
