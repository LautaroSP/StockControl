namespace StockControl.Dominio;

/// <summary>Pide un comprobante a ARCA y deja el estado correcto: Autorizado, Rechazado o Pendiente. Nunca duplica número.</summary>
public static class EmisorComprobante
{
    public static async Task<ComprobanteFiscal> Emitir(
        IClienteArca cliente,
        ConfiguracionFiscal config,
        ComprobanteFiscal comprobante,
        ComprobanteFiscal? origen,
        CancellationToken ct = default)
    {
        var tipo = TipoComprobanteFiscal.Codigo(comprobante.Tipo);
        var ultimo = await cliente.UltimoAutorizado(config.Cuit, config.PuntoVenta, tipo, ct);
        comprobante.PuntoVenta = config.PuntoVenta;
        comprobante.Numero = ultimo + 1;
        comprobante.Estado = EstadoComprobanteFiscal.Pendiente;
        await Pedir(cliente, config, comprobante, origen, ct);
        return comprobante;
    }

    /// <summary>Reintenta un pendiente: primero consulta el número que ya tenía; solo si ARCA no lo emitió pide de nuevo.</summary>
    public static async Task Reintentar(
        IClienteArca cliente,
        ConfiguracionFiscal config,
        ComprobanteFiscal pendiente,
        ComprobanteFiscal? origen,
        CancellationToken ct = default)
    {
        if (pendiente.Estado != EstadoComprobanteFiscal.Pendiente)
            throw new ErrorNegocio("El comprobante no está pendiente.");
        var tipo = TipoComprobanteFiscal.Codigo(pendiente.Tipo);
        if (pendiente.Numero > 0)
        {
            var emitido = await cliente.Consultar(config.Cuit, config.PuntoVenta, tipo, pendiente.Numero, ct);
            if (emitido != null)
            {
                Aplicar(pendiente, emitido);
                return;
            }
        }
        pendiente.Numero = await cliente.UltimoAutorizado(config.Cuit, config.PuntoVenta, tipo, ct) + 1;
        await Pedir(cliente, config, pendiente, origen, ct);
    }

    private static async Task Pedir(
        IClienteArca cliente, ConfiguracionFiscal config, ComprobanteFiscal c, ComprobanteFiscal? origen, CancellationToken ct)
    {
        var tipo = TipoComprobanteFiscal.Codigo(c.Tipo);
        var solicitud = new SolicitudArca
        {
            Cuit = config.Cuit,
            PuntoVenta = config.PuntoVenta,
            Tipo = tipo,
            Numero = c.Numero,
            Importe = c.Importe,
            Fecha = DateOnly.FromDateTime(c.Fecha.ToOffset(TimeSpan.FromHours(-3)).DateTime),
            Asociado = origen == null ? null : new ComprobanteAsociado
            {
                Tipo = TipoComprobanteFiscal.Codigo(origen.Tipo),
                PuntoVenta = origen.PuntoVenta,
                Numero = origen.Numero
            }
        };
        try
        {
            Aplicar(c, await cliente.Solicitar(solicitud, ct));
        }
        catch (ExcepcionArca ex) when (ex.Transitoria)
        {
            await Recuperar(cliente, config, c, tipo, ct);
        }
    }

    /// <summary>Se perdió la respuesta: ARCA pudo haber emitido igual. Si no se puede confirmar, queda Pendiente.</summary>
    private static async Task Recuperar(IClienteArca cliente, ConfiguracionFiscal config, ComprobanteFiscal c, int tipo, CancellationToken ct)
    {
        try
        {
            var emitido = await cliente.Consultar(config.Cuit, config.PuntoVenta, tipo, c.Numero, ct);
            if (emitido != null)
                Aplicar(c, emitido);
        }
        catch (ExcepcionArca)
        {
            c.Observacion = "ARCA no respondió. Reintentá más tarde.";
        }
    }

    private static void Aplicar(ComprobanteFiscal c, RespuestaArca r)
    {
        c.Observacion = r.Observacion;
        if (!r.Aprobado)
        {
            c.Estado = EstadoComprobanteFiscal.Rechazado;
            return;
        }
        c.Estado = EstadoComprobanteFiscal.Autorizado;
        c.Cae = r.Cae;
        c.VencimientoCae = r.VencimientoCae;
        if (r.Numero > 0) c.Numero = r.Numero;
    }
}
