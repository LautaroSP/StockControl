using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioTiendaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 10, 26, 12, 0, 0, TimeSpan.FromHours(-3));

    private static Local Tienda(string estado = ServicioTienda.Activa, string abono = ServicioAbono.AlDia, int diasAbono = 20) => new()
    {
        IdLocal = 1,
        Nombre = "Almagro",
        EstadoAbono = abono,
        Vence = Ahora.AddDays(diasAbono),
        AliasTienda = "almagro",
        EstadoTienda = estado
    };

    [Theory]
    [InlineData("Almacen-Don-Pepe", "almacen-don-pepe")]
    [InlineData("  kiosco24 ", "kiosco24")]
    public void Alias_se_guarda_en_minusculas_y_sin_espacios_alrededor(string alias, string esperado)
    {
        Assert.Equal(esperado, ServicioTienda.NormalizarAlias(alias));
    }

    [Theory]
    [InlineData("don pepe")]
    [InlineData("-pepe")]
    [InlineData("pepe-")]
    [InlineData("pepe--sur")]
    [InlineData("almacén")]
    [InlineData("ab")]
    [InlineData("pepe/1")]
    public void Alias_con_caracteres_invalidos_se_rechaza(string alias)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioTienda.NormalizarAlias(alias));
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("API")]
    [InlineData("tienda")]
    [InlineData("login")]
    public void Alias_reservado_se_rechaza(string alias)
    {
        var error = Assert.Throws<ErrorNegocio>(() => ServicioTienda.NormalizarAlias(alias));

        Assert.Contains("reservado", error.Message);
    }

    [Fact]
    public void Alias_vacio_deja_la_tienda_sin_alias()
    {
        Assert.Null(ServicioTienda.NormalizarAlias("   "));
    }

    [Fact]
    public void Alias_tomado_por_otro_local_se_rechaza()
    {
        var local = Tienda();

        Assert.Throws<ErrorNegocio>(() => ServicioTienda.AsignarAlias(local, "pepe", tomadoPorOtro: true));
        Assert.Equal("almagro", local.AliasTienda);
    }

    [Fact]
    public void Alias_libre_se_asigna()
    {
        var local = Tienda();

        ServicioTienda.AsignarAlias(local, "pepe", tomadoPorOtro: false);

        Assert.Equal("pepe", local.AliasTienda);
    }

    [Fact]
    public void Activar_sin_alias_se_rechaza()
    {
        var local = Tienda(ServicioTienda.Inactiva);
        local.AliasTienda = null;

        Assert.Throws<ErrorNegocio>(() => ServicioTienda.CambiarEstado(local, ServicioTienda.Activa));
    }

    [Fact]
    public void Estado_desconocido_se_rechaza()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioTienda.CambiarEstado(Tienda(), "abierta"));
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    public void Ubicacion_fuera_de_rango_se_rechaza(double latitud, double longitud)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioTienda.AsignarUbicacion(Tienda(), (decimal)latitud, (decimal)longitud));
    }

    [Fact]
    public void Ubicacion_con_un_solo_dato_se_rechaza()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioTienda.AsignarUbicacion(Tienda(), -34.6m, null));
    }

    [Fact]
    public void Ubicacion_valida_se_guarda_y_muestra_el_mapa()
    {
        var local = Tienda();

        ServicioTienda.AsignarUbicacion(local, -34.6037219m, -58.3815704m);

        Assert.Equal(-34.603722m, local.Latitud);
        Assert.Equal(-58.381570m, local.Longitud);
        Assert.True(ServicioTienda.MuestraMapa(local));
    }

    [Fact]
    public void Sin_ubicacion_no_hay_mapa()
    {
        var local = Tienda();

        ServicioTienda.AsignarUbicacion(local, null, null);

        Assert.False(ServicioTienda.MuestraMapa(local));
    }

    [Fact]
    public void WhatsApp_se_guarda_en_formato_wa_me()
    {
        var local = Tienda();

        ServicioTienda.AsignarDatos(local, "Av. Rivadavia 1234", "Lun a sáb 9 a 20", "011 15 1234-5678", true);

        Assert.Equal("5491112345678", local.WhatsApp);
        Assert.True(local.HaceEnvios);
    }

    [Fact]
    public void Tienda_activa_y_al_dia_muestra_el_catalogo()
    {
        Assert.Equal(ServicioTienda.VistaCatalogo, ServicioTienda.Vista(Tienda(), Ahora));
    }

    [Fact]
    public void Tienda_inactiva_no_resuelve()
    {
        Assert.Null(ServicioTienda.Vista(Tienda(ServicioTienda.Inactiva), Ahora));
    }

    [Fact]
    public void Local_de_baja_no_resuelve()
    {
        var local = Tienda();
        local.Activo = false;

        Assert.Null(ServicioTienda.Vista(local, Ahora));
    }

    [Fact]
    public void Tienda_pausada_muestra_la_pagina_de_no_disponible()
    {
        Assert.Equal(ServicioTienda.VistaPausada, ServicioTienda.Vista(Tienda(ServicioTienda.Pausada), Ahora));
    }

    [Fact]
    public void Abono_vencido_cierra_la_tienda()
    {
        Assert.Equal(ServicioTienda.VistaCerrada, ServicioTienda.Vista(Tienda(diasAbono: -1), Ahora));
    }

    [Fact]
    public void Abono_pendiente_cierra_la_tienda()
    {
        Assert.Equal(ServicioTienda.VistaCerrada, ServicioTienda.Vista(Tienda(abono: ServicioAbono.Pendiente), Ahora));
    }

    [Fact]
    public void Abono_vencido_manda_sobre_la_pausa()
    {
        Assert.Equal(ServicioTienda.VistaCerrada, ServicioTienda.Vista(Tienda(ServicioTienda.Pausada, diasAbono: -1), Ahora));
    }

    [Fact]
    public void Tienda_en_prueba_muestra_el_catalogo()
    {
        Assert.Equal(ServicioTienda.VistaCatalogo, ServicioTienda.Vista(Tienda(abono: ServicioAbono.Trial, diasAbono: 3), Ahora));
    }
}
