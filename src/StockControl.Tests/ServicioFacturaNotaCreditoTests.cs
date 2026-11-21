using System.Text;
using System.Text.Json;
using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioFacturaNotaCreditoTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 11, 21, 15, 0, 0, TimeSpan.Zero);

    private static ConfiguracionFiscal Config() => new() { IdLocal = 1, Cuit = "23401337679", PuntoVenta = 3, Activa = true };

    private static async Task<(ClienteArcaFalso Arca, ComprobanteFiscal Factura)> FacturaAutorizada(string alcance)
    {
        var arca = new ClienteArcaFalso();
        var ventas = new List<InformeVenta>
        {
            new() { IdInformeVenta = 1, IdLocal = 1, Total = 100, NroCaja = 1, IdCierre = 7 },
            new() { IdInformeVenta = 2, IdLocal = 1, Total = 250, NroCaja = 1, IdCierre = 7 }
        };
        var modo = alcance == AlcanceComprobanteFiscal.Caja ? ModoFacturacion.Global : ModoFacturacion.PorVenta;
        var r = await ServicioFacturacion.FacturarCaja(arca, Config(), Roles.Dueno, 7, ventas,
            new HashSet<int>(), modo, 417_288m, Ahora);
        var factura = r.Comprobantes[0];
        factura.Id = 10;
        return (arca, factura);
    }

    [Fact]
    public async Task Anular_venta_facturada_por_venta_emite_nc_total()
    {
        var (arca, factura) = await FacturaAutorizada(AlcanceComprobanteFiscal.Venta);
        var nota = await ServicioNotaCredito.Emitir(arca, Config(), factura, factura.Ventas[0], Ahora);
        Assert.Equal(TipoComprobanteFiscal.NotaCreditoC, nota.Tipo);
        Assert.Equal(EstadoComprobanteFiscal.Autorizado, nota.Estado);
        Assert.Equal(factura.Importe, nota.Importe);
        Assert.Equal(factura.Numero, arca.Solicitudes.Last().Asociado!.Numero);
    }

    [Fact]
    public async Task Anular_venta_de_factura_global_emite_nc_parcial()
    {
        var (arca, factura) = await FacturaAutorizada(AlcanceComprobanteFiscal.Caja);
        var nota = await ServicioNotaCredito.Emitir(arca, Config(), factura, factura.Ventas[1], Ahora);
        Assert.Equal(250m, nota.Importe);
        Assert.True(nota.Importe < factura.Importe);
        Assert.Equal(factura.Id, nota.IdComprobanteOrigen);
        Assert.Equal(2, nota.IdInformeVenta);
    }

    [Fact]
    public async Task Nota_de_credito_queda_pendiente_si_arca_no_esta_disponible()
    {
        var (_, factura) = await FacturaAutorizada(AlcanceComprobanteFiscal.Venta);
        var nota = await ServicioNotaCredito.Emitir(null, Config(), factura, factura.Ventas[0], Ahora);
        Assert.Equal(EstadoComprobanteFiscal.Pendiente, nota.Estado);
        Assert.Equal(0, nota.Numero);
    }

    [Fact]
    public async Task Nota_de_credito_pendiente_se_emite_al_reintentar()
    {
        var (arca, factura) = await FacturaAutorizada(AlcanceComprobanteFiscal.Venta);
        var nota = await ServicioNotaCredito.Emitir(null, Config(), factura, factura.Ventas[0], Ahora);
        await EmisorComprobante.Reintentar(arca, Config(), nota, factura);
        Assert.Equal(EstadoComprobanteFiscal.Autorizado, nota.Estado);
    }

    [Fact]
    public async Task Nota_de_credito_exige_factura_autorizada()
    {
        var (arca, factura) = await FacturaAutorizada(AlcanceComprobanteFiscal.Venta);
        factura.Estado = EstadoComprobanteFiscal.Pendiente;
        await Assert.ThrowsAsync<ErrorNegocio>(() =>
            ServicioNotaCredito.Emitir(arca, Config(), factura, factura.Ventas[0], Ahora));
    }

    [Fact]
    public void Anular_venta_no_facturada_de_caja_cerrada_sigue_prohibido()
    {
        var venta = new InformeVenta { IdCierre = 7 };
        Assert.Throws<ErrorNegocio>(() => ServicioAnularVenta.Anular(venta, [], "x", facturada: false, rol: Roles.Dueno));
    }

    [Fact]
    public void Anular_venta_facturada_de_caja_cerrada_la_puede_anular_duenio_o_socio()
    {
        var venta = new InformeVenta { IdCierre = 7 };
        ServicioAnularVenta.Anular(venta, [], "x", facturada: true, rol: Roles.Socio);
    }

    [Fact]
    public void Anular_venta_facturada_de_caja_cerrada_no_la_puede_anular_admin_ni_empleado()
    {
        var venta = new InformeVenta { IdCierre = 7 };
        Assert.Throws<ErrorNegocio>(() => ServicioAnularVenta.Anular(venta, [], "x", facturada: true, rol: Roles.Empleado));
        Assert.Throws<ErrorNegocio>(() => ServicioAnularVenta.Anular(venta, [], "x", facturada: true, rol: Roles.Admin));
    }

    [Fact]
    public async Task Comprobante_autorizado_arma_el_qr_de_arca()
    {
        var (_, factura) = await FacturaAutorizada(AlcanceComprobanteFiscal.Venta);
        var url = QrArca.Url(Config(), factura)!;
        Assert.StartsWith(QrArca.UrlBase, url);
        var json = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(url[QrArca.UrlBase.Length..])));
        Assert.Equal(11, json.RootElement.GetProperty("tipoCmp").GetInt32());
        Assert.Equal(23401337679L, json.RootElement.GetProperty("cuit").GetInt64());
        Assert.Equal(99, json.RootElement.GetProperty("tipoDocRec").GetInt32());
    }

    [Fact]
    public void Comprobante_pendiente_no_tiene_qr()
    {
        Assert.Null(QrArca.Url(Config(), new ComprobanteFiscal { Estado = EstadoComprobanteFiscal.Pendiente }));
    }
}
