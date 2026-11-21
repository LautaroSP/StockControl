namespace StockControl.Api.Fiscal;

/// <summary>
/// Configuración de ARCA. El certificado y la clave privada son de StockControl (delegado por cada local) y
/// viven SOLO en el entorno: Arca__Certificado y Arca__ClavePrivada (PEM), o Arca__CertificadoRuta y Arca__ClavePrivadaRuta
/// (archivos fuera del repo). Arca__Ambiente = homologacion | produccion. Arca__TopeConsumidorFinal opcional.
/// </summary>
public sealed class OpcionesArca
{
    public const string Homologacion = "homologacion";
    public const string Produccion = "produccion";

    public string Ambiente { get; }
    public decimal TopeConsumidorFinal { get; }
    private readonly string _certificado;
    private readonly string _clavePrivada;

    public OpcionesArca(IConfiguration config)
    {
        Ambiente = string.Equals(config["Arca:Ambiente"], Produccion, StringComparison.OrdinalIgnoreCase) ? Produccion : Homologacion;
        TopeConsumidorFinal = decimal.TryParse(config["Arca:TopeConsumidorFinal"], System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out var tope) && tope > 0
            ? tope
            : StockControl.Dominio.ServicioFacturacion.TopeConsumidorFinalReferencia;
        _certificado = Leer(config["Arca:Certificado"], config["Arca:CertificadoRuta"]);
        _clavePrivada = Leer(config["Arca:ClavePrivada"], config["Arca:ClavePrivadaRuta"]);
    }

    public bool Disponible => _certificado.Length > 0 && _clavePrivada.Length > 0;

    public string UrlWsaa => Ambiente == Produccion
        ? "https://wsaa.afip.gov.ar/ws/services/LoginCms"
        : "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";

    public string UrlWsfe => Ambiente == Produccion
        ? "https://servicios1.afip.gov.ar/wsfev1/service.asmx"
        : "https://wswhomo.afip.gov.ar/wsfev1/service.asmx";

    public (string Certificado, string ClavePrivada) Credenciales() => (_certificado, _clavePrivada);

    /// <summary>Nunca imprime el certificado ni la clave.</summary>
    public override string ToString() => $"Arca[{Ambiente}, disponible={Disponible}]";

    private static string Leer(string? contenido, string? ruta)
    {
        if (!string.IsNullOrWhiteSpace(contenido)) return contenido.Replace("\\n", "\n").Trim();
        return !string.IsNullOrWhiteSpace(ruta) && File.Exists(ruta) ? File.ReadAllText(ruta).Trim() : "";
    }
}
