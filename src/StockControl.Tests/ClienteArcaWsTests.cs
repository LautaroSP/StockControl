using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Configuration;
using StockControl.Api.Fiscal;
using StockControl.Dominio;

namespace StockControl.Tests;

/// <summary>Piezas del cliente real de ARCA que se pueden probar sin red ni credenciales.</summary>
public class ClienteArcaWsTests
{
    private static (string Certificado, string Clave) CertificadoDePrueba()
    {
        using var rsa = RSA.Create(2048);
        var pedido = new CertificateRequest("CN=StockControl prueba", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var cert = pedido.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        return (cert.ExportCertificatePem(), rsa.ExportPkcs8PrivateKeyPem());
    }

    [Fact]
    public void Arca_firma_el_pedido_de_acceso_con_el_certificado()
    {
        var (cert, clave) = CertificadoDePrueba();
        var pedido = ClienteWsaa.ArmarPedido(new DateTimeOffset(2026, 11, 21, 12, 0, 0, TimeSpan.Zero));
        var firmado = ClienteWsaa.Firmar(cert, clave, pedido);

        var cms = new SignedCms();
        cms.Decode(firmado);
        cms.CheckSignature(verifySignatureOnly: true);
        Assert.Equal(pedido, cms.ContentInfo.Content);
        Assert.Contains("<service>wsfe</service>", Encoding.UTF8.GetString(pedido));
    }

    [Fact]
    public void Arca_solicitud_de_factura_c_va_a_consumidor_final_sin_iva()
    {
        var xml = ClienteWsfe.CuerpoSolicitud(new SolicitudArca
        {
            Cuit = "23401337679", PuntoVenta = 3, Tipo = 11, Numero = 5, Importe = 1234.5m,
            Fecha = new DateOnly(2026, 11, 21)
        }, "<ar:Auth/>");
        Assert.Contains("<ar:CbteTipo>11</ar:CbteTipo>", xml);
        Assert.Contains("<ar:DocTipo>99</ar:DocTipo><ar:DocNro>0</ar:DocNro>", xml);
        Assert.Contains("<ar:ImpTotal>1234.50</ar:ImpTotal>", xml);
        Assert.Contains("<ar:ImpIVA>0.00</ar:ImpIVA>", xml);
        Assert.Contains("<ar:CbteFch>20261121</ar:CbteFch>", xml);
        Assert.DoesNotContain("CbtesAsoc", xml);
    }

    [Fact]
    public void Arca_solicitud_de_nota_de_credito_c_incluye_la_factura_asociada()
    {
        var xml = ClienteWsfe.CuerpoSolicitud(new SolicitudArca
        {
            Cuit = "23401337679", PuntoVenta = 3, Tipo = 13, Numero = 1, Importe = 100m,
            Fecha = new DateOnly(2026, 11, 21),
            Asociado = new ComprobanteAsociado { Tipo = 11, PuntoVenta = 3, Numero = 5 }
        }, "<ar:Auth/>");
        Assert.Contains("<ar:Tipo>11</ar:Tipo><ar:PtoVta>3</ar:PtoVta><ar:Nro>5</ar:Nro>", xml);
    }

    [Fact]
    public void Arca_sin_certificado_en_el_entorno_no_esta_disponible()
    {
        var opciones = new OpcionesArca(new ConfigurationBuilder().Build());
        Assert.False(opciones.Disponible);
        Assert.Equal(OpcionesArca.Homologacion, opciones.Ambiente);
        Assert.Equal(ServicioFacturacion.TopeConsumidorFinalReferencia, opciones.TopeConsumidorFinal);
    }

    [Fact]
    public void Arca_no_muestra_certificado_ni_clave_al_imprimirse()
    {
        var (cert, clave) = CertificadoDePrueba();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Arca:Certificado"] = cert, ["Arca:ClavePrivada"] = clave, ["Arca:TopeConsumidorFinal"] = "500000"
        }).Build();
        var opciones = new OpcionesArca(config);
        Assert.True(opciones.Disponible);
        Assert.Equal(500000m, opciones.TopeConsumidorFinal);
        Assert.DoesNotContain("PRIVATE", opciones.ToString());
        Assert.DoesNotContain("CERTIFICATE", opciones.ToString());
    }
}
