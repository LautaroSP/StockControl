using StockControl.Dominio;

namespace StockControl.Tests;

public class ConfiguracionFiscalTests
{
    [Theory]
    [InlineData("23-40133767-9", true)]
    [InlineData("23401337679", true)]
    [InlineData("23401337670", false)]
    [InlineData("1234", false)]
    [InlineData("", false)]
    public void Cuit_valida_digito_verificador(string cuit, bool esperado) =>
        Assert.Equal(esperado, Cuit.EsValido(cuit));

    [Fact]
    public void Configuracion_fiscal_exige_cuit_y_punto_de_venta()
    {
        var c = new ConfiguracionFiscal { IdLocal = 1 };
        Assert.Throws<ErrorNegocio>(() => ConfiguracionFiscal.Guardar(c, "20111111111", 1, true, false));
        Assert.Throws<ErrorNegocio>(() => ConfiguracionFiscal.Guardar(c, "23-40133767-9", 0, true, false));
        Assert.Throws<ErrorNegocio>(() => ConfiguracionFiscal.Guardar(c, "23-40133767-9", 100000, true, false));
    }

    [Fact]
    public void Configuracion_fiscal_guarda_el_cuit_sin_guiones()
    {
        var c = new ConfiguracionFiscal { IdLocal = 1 };
        ConfiguracionFiscal.Guardar(c, "23-40133767-9", 4, true, true);
        Assert.Equal("23401337679", c.Cuit);
        Assert.Equal(4, c.PuntoVenta);
        Assert.True(c.FacturaGlobalPorDefecto);
    }

    [Fact]
    public void Facturacion_fiscal_solo_para_duenio_y_socio()
    {
        Assert.True(Permisos.PuedeFacturar(Roles.Dueno));
        Assert.True(Permisos.PuedeFacturar(Roles.Socio));
        Assert.False(Permisos.PuedeFacturar(Roles.Empleado));
    }

    [Fact]
    public void Entidades_fiscales_no_guardan_certificado_ni_clave()
    {
        var tipos = new[] { typeof(ConfiguracionFiscal), typeof(ComprobanteFiscal), typeof(ComprobanteFiscalVenta) };
        var nombres = tipos.SelectMany(t => t.GetProperties()).Select(p => p.Name.ToLowerInvariant());
        Assert.DoesNotContain(nombres, n => n.Contains("clave") || n.Contains("certificado") || n.Contains("privada") || n.Contains("token"));
    }
}
