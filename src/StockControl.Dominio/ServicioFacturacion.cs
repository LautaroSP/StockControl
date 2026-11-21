namespace StockControl.Dominio;

public enum ModoFacturacion
{
    PorVenta,
    Global
}

public class VentaOmitida
{
    public int IdInformeVenta { get; init; }
    public string Motivo { get; init; } = "";
}

public class ResultadoFacturacion
{
    public List<ComprobanteFiscal> Comprobantes { get; } = new();
    public List<VentaOmitida> Omitidas { get; } = new();
}

/// <summary>Factura una caja cerrada (Factura C a consumidor final). No guarda nada: devuelve los comprobantes para que el API los persista.</summary>
public static class ServicioFacturacion
{
    /// <summary>
    /// Tope por comprobante para consumidor final sin identificar. Valor de referencia (2025, ARCA), configurable en el entorno
    /// (Arca__TopeConsumidorFinal). VERIFICAR en homologación antes de producción: ARCA lo actualiza.
    /// </summary>
    public const decimal TopeConsumidorFinalReferencia = 417_288m;

    public static async Task<ResultadoFacturacion> FacturarCaja(
        IClienteArca cliente,
        ConfiguracionFiscal? config,
        string rol,
        int idCierre,
        IReadOnlyList<InformeVenta> ventasSeleccionadas,
        IReadOnlySet<int> yaFacturadas,
        ModoFacturacion modo,
        decimal tope,
        DateTimeOffset ahora,
        CancellationToken ct = default)
    {
        if (!Permisos.PuedeFacturar(rol))
            throw new ErrorNegocio("Solo el dueño o un socio pueden facturar.");
        if (config == null || !config.Activa)
            throw new ErrorNegocio("Configurá el CUIT y el punto de venta antes de facturar.");
        var resultado = new ResultadoFacturacion();
        var pendientes = Filtrar(config, idCierre, ventasSeleccionadas, yaFacturadas, resultado);
        if (pendientes.Count == 0)
            throw new ErrorNegocio("No hay ventas sin facturar en esa caja.");

        if (modo == ModoFacturacion.Global)
            await FacturarGlobal(cliente, config, idCierre, pendientes, tope, ahora, resultado, ct);
        else
            await FacturarPorVenta(cliente, config, idCierre, pendientes, tope, ahora, resultado, ct);
        return resultado;
    }

    private static List<InformeVenta> Filtrar(
        ConfiguracionFiscal config, int idCierre, IReadOnlyList<InformeVenta> ventas,
        IReadOnlySet<int> yaFacturadas, ResultadoFacturacion resultado)
    {
        if (ventas.Any(v => v.IdCierre == null))
            throw new ErrorNegocio("Solo se factura una caja cerrada.");
        if (ventas.Any(v => v.IdCierre != idCierre))
            throw new ErrorNegocio("Hay ventas que no son de esa caja.");
        if (ventas.Any(v => v.IdLocal != config.IdLocal))
            throw new ErrorNegocio("Hay ventas de otro local.");
        var pendientes = new List<InformeVenta>();
        foreach (var v in ventas)
        {
            if (yaFacturadas.Contains(v.IdInformeVenta)) continue;
            if (v.Total <= 0)
                resultado.Omitidas.Add(new VentaOmitida { IdInformeVenta = v.IdInformeVenta, Motivo = "Importe cero." });
            else
                pendientes.Add(v);
        }
        return pendientes;
    }

    private static async Task FacturarGlobal(
        IClienteArca cliente, ConfiguracionFiscal config, int idCierre, List<InformeVenta> ventas,
        decimal tope, DateTimeOffset ahora, ResultadoFacturacion resultado, CancellationToken ct)
    {
        var total = ventas.Sum(v => v.Total);
        if (total > tope)
            throw new ErrorNegocio($"El total ($ {total:N2}) supera el tope para consumidor final ($ {tope:N2}). Facturá por venta.");
        var comprobante = NuevaFactura(config, idCierre, ventas[0].NroCaja, total, ahora, AlcanceComprobanteFiscal.Caja);
        comprobante.Ventas.AddRange(ventas.Select(v => FilaVenta(config, v)));
        await Emitir(cliente, config, comprobante, resultado, ct);
    }

    private static async Task FacturarPorVenta(
        IClienteArca cliente, ConfiguracionFiscal config, int idCierre, List<InformeVenta> ventas,
        decimal tope, DateTimeOffset ahora, ResultadoFacturacion resultado, CancellationToken ct)
    {
        foreach (var v in ventas)
        {
            if (v.Total > tope)
            {
                resultado.Omitidas.Add(new VentaOmitida { IdInformeVenta = v.IdInformeVenta, Motivo = "Supera el tope para consumidor final: hay que identificar al cliente." });
                continue;
            }
            var comprobante = NuevaFactura(config, idCierre, v.NroCaja, v.Total, ahora, AlcanceComprobanteFiscal.Venta);
            comprobante.Ventas.Add(FilaVenta(config, v));
            await Emitir(cliente, config, comprobante, resultado, ct);
            // Con un pendiente no se sigue: el próximo número depende de si ARCA lo emitió.
            if (comprobante.Estado == EstadoComprobanteFiscal.Pendiente) break;
        }
    }

    private static async Task Emitir(
        IClienteArca cliente, ConfiguracionFiscal config, ComprobanteFiscal comprobante,
        ResultadoFacturacion resultado, CancellationToken ct)
    {
        try
        {
            await EmisorComprobante.Emitir(cliente, config, comprobante, null, ct);
        }
        catch (ExcepcionArca ex)
        {
            throw new ErrorNegocio($"No se pudo facturar: {ex.Message}");
        }
        // Una factura rechazada no deja ventas marcadas como facturadas.
        if (comprobante.Estado == EstadoComprobanteFiscal.Rechazado)
            comprobante.Ventas.Clear();
        resultado.Comprobantes.Add(comprobante);
    }

    private static ComprobanteFiscal NuevaFactura(
        ConfiguracionFiscal config, int idCierre, int nroCaja, decimal importe, DateTimeOffset ahora, string alcance) =>
        new()
        {
            IdLocal = config.IdLocal,
            IdCierre = idCierre,
            NroCaja = nroCaja,
            Tipo = TipoComprobanteFiscal.FacturaC,
            Importe = importe,
            Fecha = ahora,
            Alcance = alcance
        };

    private static ComprobanteFiscalVenta FilaVenta(ConfiguracionFiscal config, InformeVenta v) =>
        new() { IdLocal = config.IdLocal, IdInformeVenta = v.IdInformeVenta, Importe = v.Total };
}
