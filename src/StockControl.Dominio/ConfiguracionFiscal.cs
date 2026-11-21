namespace StockControl.Dominio;

/// <summary>Datos fiscales del local para facturar con ARCA. El certificado NO va acá: es de StockControl y vive en el entorno.</summary>
public class ConfiguracionFiscal
{
    public int IdLocal { get; set; }
    public string Cuit { get; set; } = "";
    public int PuntoVenta { get; set; }
    public bool Activa { get; set; }
    /// <summary>Propuesta de la pantalla: una factura global por caja en lugar de una por venta.</summary>
    public bool FacturaGlobalPorDefecto { get; set; }

    public const int PuntoVentaMaximo = 99999;

    public static void Validar(string cuit, int puntoVenta)
    {
        if (!StockControl.Dominio.Cuit.EsValido(cuit))
            throw new ErrorNegocio("El CUIT no es válido.");
        if (puntoVenta < 1 || puntoVenta > PuntoVentaMaximo)
            throw new ErrorNegocio("El punto de venta debe estar entre 1 y 99999.");
    }

    public static void Guardar(ConfiguracionFiscal config, string cuit, int puntoVenta, bool activa, bool globalPorDefecto)
    {
        Validar(cuit, puntoVenta);
        config.Cuit = StockControl.Dominio.Cuit.Normalizar(cuit);
        config.PuntoVenta = puntoVenta;
        config.Activa = activa;
        config.FacturaGlobalPorDefecto = globalPorDefecto;
    }
}
