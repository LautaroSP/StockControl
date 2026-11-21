namespace StockControl.Dominio;

/// <summary>Nota de crédito C al anular una venta facturada. Total si la factura era de la venta; parcial (solo esa venta) si era la global.</summary>
public static class ServicioNotaCredito
{
    /// <summary>Si ARCA no está disponible la nota queda Pendiente (sin número): se reintenta después.</summary>
    public static async Task<ComprobanteFiscal> Emitir(
        IClienteArca? cliente,
        ConfiguracionFiscal config,
        ComprobanteFiscal factura,
        ComprobanteFiscalVenta fila,
        DateTimeOffset ahora,
        CancellationToken ct = default)
    {
        if (factura.Tipo != TipoComprobanteFiscal.FacturaC || factura.Estado != EstadoComprobanteFiscal.Autorizado)
            throw new ErrorNegocio("La factura todavía no está autorizada por ARCA. Resolvela antes de anular la venta.");
        var nota = new ComprobanteFiscal
        {
            IdLocal = factura.IdLocal,
            IdCierre = factura.IdCierre,
            NroCaja = factura.NroCaja,
            Tipo = TipoComprobanteFiscal.NotaCreditoC,
            PuntoVenta = config.PuntoVenta,
            Importe = fila.Importe,
            Fecha = ahora,
            Alcance = AlcanceComprobanteFiscal.Venta,
            IdComprobanteOrigen = factura.Id,
            IdInformeVenta = fila.IdInformeVenta,
            Estado = EstadoComprobanteFiscal.Pendiente
        };
        if (cliente == null)
        {
            nota.Observacion = "ARCA no está configurado. Emitir la nota de crédito cuando esté disponible.";
            return nota;
        }
        try
        {
            await EmisorComprobante.Emitir(cliente, config, nota, factura, ct);
        }
        catch (ExcepcionArca ex)
        {
            nota.Observacion = ex.Message;
        }
        return nota;
    }
}
