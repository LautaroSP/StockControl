using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCarritosTests
{
    [Fact]
    public void Iniciar_crea_cuatro_carritos_vacios()
    {
        var carritos = ServicioCarritos.Iniciar();

        Assert.Equal(4, carritos.Count);
        Assert.All(carritos, c => Assert.Empty(c.Items));
    }

    [Fact]
    public void Crear_agrega_uno_y_devuelve_su_indice()
    {
        var carritos = ServicioCarritos.Iniciar();

        var indice = ServicioCarritos.Crear(carritos);

        Assert.Equal(4, indice);
        Assert.Equal(5, carritos.Count);
    }

    [Fact]
    public void Cerrar_un_fijo_falla()
    {
        var carritos = ServicioCarritos.Iniciar();

        var ex = Assert.Throws<ErrorNegocio>(() => ServicioCarritos.Cerrar(carritos, 2, confirmar: true));

        Assert.Contains("primeros cuatro", ex.Message);
        Assert.Equal(4, carritos.Count);
    }

    [Fact]
    public void Cerrar_el_quinto_sin_items_lo_saca()
    {
        var carritos = ServicioCarritos.Iniciar();
        ServicioCarritos.Crear(carritos);

        var indice = ServicioCarritos.Cerrar(carritos, 4, confirmar: false);

        Assert.Equal(3, indice);
        Assert.Equal(4, carritos.Count);
    }

    [Fact]
    public void Cerrar_con_items_sin_confirmar_falla()
    {
        var carritos = ServicioCarritos.Iniciar();
        var extra = ServicioCarritos.Crear(carritos);
        carritos[extra].Items.Add(new ItemCarrito { Nombre = "Coca", Cantidad = 1 });

        var ex = Assert.Throws<ErrorNegocio>(() => ServicioCarritos.Cerrar(carritos, extra, confirmar: false));

        Assert.Contains("productos", ex.Message);
        Assert.Equal(5, carritos.Count);
    }

    [Fact]
    public void Cerrar_con_items_confirmado_lo_saca()
    {
        var carritos = ServicioCarritos.Iniciar();
        var extra = ServicioCarritos.Crear(carritos);
        carritos[extra].Items.Add(new ItemCarrito { Nombre = "Coca", Cantidad = 1 });

        var indice = ServicioCarritos.Cerrar(carritos, extra, confirmar: true);

        Assert.Equal(3, indice);
        Assert.Equal(4, carritos.Count);
    }

    [Fact]
    public void AlCobrar_en_fijo_vacia_y_mantiene_cuatro()
    {
        var carritos = ServicioCarritos.Iniciar();
        carritos[0].Items.Add(new ItemCarrito { Nombre = "Coca", Cantidad = 1 });
        carritos[0].DescuentoActivo = true;
        carritos[0].Descuento = 10;
        carritos[0].AlCosto = true;
        carritos[0].ImprimirTicket = true;

        var indice = ServicioCarritos.AlCobrar(carritos, 0);

        Assert.Equal(0, indice);
        Assert.Equal(4, carritos.Count);
        Assert.Empty(carritos[0].Items);
        Assert.False(carritos[0].DescuentoActivo);
        Assert.Equal(0, carritos[0].Descuento);
        Assert.False(carritos[0].AlCosto);
        Assert.True(carritos[0].ImprimirTicket);
    }

    [Fact]
    public void AlCobrar_en_extra_cierra_ese_carrito()
    {
        var carritos = ServicioCarritos.Iniciar();
        var extra = ServicioCarritos.Crear(carritos);
        carritos[extra].Items.Add(new ItemCarrito { Nombre = "Coca", Cantidad = 1 });

        var indice = ServicioCarritos.AlCobrar(carritos, extra);

        Assert.Equal(3, indice);
        Assert.Equal(4, carritos.Count);
    }

    [Fact]
    public void Atajo_F3_selecciona_el_tercero()
    {
        var carritos = ServicioCarritos.Iniciar();

        Assert.Equal(2, ServicioCarritos.Atajo(carritos, 3, 0));
    }

    [Fact]
    public void Atajo_fuera_de_rango_no_cambia()
    {
        var carritos = ServicioCarritos.Iniciar();

        Assert.Equal(1, ServicioCarritos.Atajo(carritos, 5, 1));
        Assert.Equal(1, ServicioCarritos.Atajo(carritos, 0, 1));
    }

    [Fact]
    public void Titulo_de_fijos_incluye_tecla()
    {
        Assert.Equal("1 F1", ServicioCarritos.Titulo(0));
        Assert.Equal("4 F4", ServicioCarritos.Titulo(3));
        Assert.Equal("5", ServicioCarritos.Titulo(4));
    }

    [Fact]
    public void PuedeCerrar_solo_extras()
    {
        Assert.False(ServicioCarritos.PuedeCerrar(3));
        Assert.True(ServicioCarritos.PuedeCerrar(4));
    }
}
