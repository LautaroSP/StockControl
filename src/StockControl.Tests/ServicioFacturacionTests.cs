using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioFacturacionTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 11, 20, 15, 0, 0, TimeSpan.Zero);
    private const decimal Tope = 417_288m;

    private static ConfiguracionFiscal Config(int idLocal = 1) => new()
    {
        IdLocal = idLocal, Cuit = "23401337679", PuntoVenta = 3, Activa = true
    };

    private static InformeVenta Venta(int id, decimal total, int? idCierre = 7, int idLocal = 1) => new()
    {
        IdInformeVenta = id, IdLocal = idLocal, Total = total, NroCaja = 1, IdCierre = idCierre
    };

    private static Task<ResultadoFacturacion> Facturar(
        ClienteArcaFalso arca, List<InformeVenta> ventas, ModoFacturacion modo,
        string rol = Roles.Dueno, ISet<int>? ya = null, decimal tope = Tope, ConfiguracionFiscal? config = null) =>
        ServicioFacturacion.FacturarCaja(arca, config ?? Config(), rol, 7, ventas,
            (IReadOnlySet<int>)(ya ?? new HashSet<int>()), modo, tope, Ahora);

    [Fact]
    public async Task Facturar_por_venta_genera_una_factura_por_cada_venta_de_la_caja()
    {
        var r = await Facturar(new ClienteArcaFalso(), [Venta(1, 100), Venta(2, 250)], ModoFacturacion.PorVenta);
        Assert.Equal(2, r.Comprobantes.Count);
        Assert.Equal([1L, 2L], r.Comprobantes.Select(c => c.Numero));
        Assert.All(r.Comprobantes, c => Assert.Single(c.Ventas));
    }

    [Fact]
    public async Task Facturar_global_genera_una_sola_factura_por_el_total()
    {
        var r = await Facturar(new ClienteArcaFalso(), [Venta(1, 100.10m), Venta(2, 250.20m)], ModoFacturacion.Global);
        var c = Assert.Single(r.Comprobantes);
        Assert.Equal(350.30m, c.Importe);
        Assert.Equal(AlcanceComprobanteFiscal.Caja, c.Alcance);
        Assert.Equal(2, c.Ventas.Count);
        Assert.Equal(c.Importe, c.Ventas.Sum(v => v.Importe));
    }

    [Fact]
    public async Task Facturar_no_repite_ventas_ya_facturadas()
    {
        var r = await Facturar(new ClienteArcaFalso(), [Venta(1, 100), Venta(2, 250)], ModoFacturacion.PorVenta, ya: new HashSet<int> { 1 });
        var c = Assert.Single(r.Comprobantes);
        Assert.Equal(2, c.Ventas[0].IdInformeVenta);
    }

    [Fact]
    public async Task Facturar_sin_ventas_pendientes_avisa()
    {
        await Assert.ThrowsAsync<ErrorNegocio>(() =>
            Facturar(new ClienteArcaFalso(), [Venta(1, 100)], ModoFacturacion.Global, ya: new HashSet<int> { 1 }));
    }

    [Fact]
    public async Task Facturar_rechaza_caja_abierta()
    {
        var ex = await Assert.ThrowsAsync<ErrorNegocio>(() =>
            Facturar(new ClienteArcaFalso(), [Venta(1, 100, idCierre: null)], ModoFacturacion.PorVenta));
        Assert.Contains("cerrada", ex.Message);
    }

    [Fact]
    public async Task Facturar_global_se_bloquea_si_supera_el_tope()
    {
        var arca = new ClienteArcaFalso();
        await Assert.ThrowsAsync<ErrorNegocio>(() =>
            Facturar(arca, [Venta(1, 300), Venta(2, 300)], ModoFacturacion.Global, tope: 500));
        Assert.Empty(arca.Solicitudes);
    }

    [Fact]
    public async Task Facturar_global_ofrece_por_venta_cuando_supera_el_tope()
    {
        var ex = await Assert.ThrowsAsync<ErrorNegocio>(() =>
            Facturar(new ClienteArcaFalso(), [Venta(1, 300), Venta(2, 300)], ModoFacturacion.Global, tope: 500));
        Assert.Contains("por venta", ex.Message);
    }

    [Fact]
    public async Task Facturar_por_venta_omite_la_venta_que_supera_el_tope()
    {
        var r = await Facturar(new ClienteArcaFalso(), [Venta(1, 600), Venta(2, 100)], ModoFacturacion.PorVenta, tope: 500);
        Assert.Single(r.Comprobantes);
        Assert.Equal(1, r.Omitidas.Single().IdInformeVenta);
    }

    [Fact]
    public async Task Facturar_guarda_cae_y_vencimiento()
    {
        var r = await Facturar(new ClienteArcaFalso(), [Venta(1, 100)], ModoFacturacion.PorVenta);
        var c = r.Comprobantes[0];
        Assert.Equal(EstadoComprobanteFiscal.Autorizado, c.Estado);
        Assert.Equal("70123456789012", c.Cae);
        Assert.Equal(new DateOnly(2026, 11, 30), c.VencimientoCae);
        Assert.Equal(TipoComprobanteFiscal.FacturaC, c.Tipo);
        Assert.Equal(3, c.PuntoVenta);
    }

    [Fact]
    public async Task Facturar_si_arca_rechaza_no_marca_las_ventas_como_facturadas()
    {
        var r = await Facturar(new ClienteArcaFalso { Rechazar = true }, [Venta(1, 100)], ModoFacturacion.PorVenta);
        var c = Assert.Single(r.Comprobantes);
        Assert.Equal(EstadoComprobanteFiscal.Rechazado, c.Estado);
        Assert.Empty(c.Ventas);
    }

    [Fact]
    public async Task Facturar_reintento_tras_timeout_no_duplica_numero()
    {
        var arca = new ClienteArcaFalso { PerderRespuestaUnaVez = true };
        var r = await Facturar(arca, [Venta(1, 100)], ModoFacturacion.PorVenta);
        var c = Assert.Single(r.Comprobantes);
        Assert.Equal(EstadoComprobanteFiscal.Autorizado, c.Estado);
        Assert.Equal(1, c.Numero);
        Assert.Single(arca.Emitidos);
    }

    [Fact]
    public async Task Facturar_con_arca_sin_confirmar_queda_pendiente_y_reintentar_no_duplica_numero()
    {
        var arca = new ClienteArcaFalso { PerderRespuestaUnaVez = true, ConsultaCaida = true };
        var r = await Facturar(arca, [Venta(1, 100), Venta(2, 100)], ModoFacturacion.PorVenta);
        var c = Assert.Single(r.Comprobantes);
        Assert.Equal(EstadoComprobanteFiscal.Pendiente, c.Estado);

        arca.ConsultaCaida = false;
        await EmisorComprobante.Reintentar(arca, Config(), c, null);
        Assert.Equal(EstadoComprobanteFiscal.Autorizado, c.Estado);
        Assert.Equal(1, c.Numero);
        Assert.Single(arca.Emitidos);
    }

    [Fact]
    public async Task Facturar_con_arca_caida_no_deja_comprobantes()
    {
        await Assert.ThrowsAsync<ErrorNegocio>(() =>
            Facturar(new ClienteArcaFalso { Caido = true }, [Venta(1, 100)], ModoFacturacion.PorVenta));
    }

    [Fact]
    public async Task Empleado_no_puede_facturar()
    {
        var arca = new ClienteArcaFalso();
        await Assert.ThrowsAsync<ErrorNegocio>(() =>
            Facturar(arca, [Venta(1, 100)], ModoFacturacion.PorVenta, rol: Roles.Empleado));
        Assert.Empty(arca.Solicitudes);
    }

    [Theory]
    [InlineData(Roles.Dueno)]
    [InlineData(Roles.Socio)]
    public async Task Duenio_y_socio_pueden_facturar(string rol)
    {
        var r = await Facturar(new ClienteArcaFalso(), [Venta(1, 100)], ModoFacturacion.PorVenta, rol: rol);
        Assert.Single(r.Comprobantes);
    }

    [Fact]
    public async Task Facturar_no_toca_otro_local()
    {
        var arca = new ClienteArcaFalso();
        await Assert.ThrowsAsync<ErrorNegocio>(() =>
            Facturar(arca, [Venta(1, 100, idLocal: 2)], ModoFacturacion.PorVenta));
        Assert.Empty(arca.Solicitudes);
    }

    [Fact]
    public async Task Facturar_exige_configuracion_fiscal_activa()
    {
        var config = Config();
        config.Activa = false;
        await Assert.ThrowsAsync<ErrorNegocio>(() =>
            Facturar(new ClienteArcaFalso(), [Venta(1, 100)], ModoFacturacion.PorVenta, config: config));
    }
}
