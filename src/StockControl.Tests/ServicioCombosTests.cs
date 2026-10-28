using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCombosTests
{
    private static readonly DateOnly Hoy = new(2026, 10, 24);

    private static Producto Prod(int id, string nombre, decimal precio) =>
        new() { Id = id, IdLocal = 1, Codigo = $"C{id}", Nombre = nombre, Cantidad = 50, Precio = precio, Costo = precio / 2 };

    private static readonly Producto Coca = Prod(1, "Coca", 2000);
    private static readonly Producto Fernet = Prod(2, "Fernet", 10000);

    private static OfertaCombo Combo(decimal precio, DateOnly? hasta = null) => new()
    {
        Id = 1, IdLocal = 1, Nombre = "2 Coca + Fernet", PrecioTotal = precio, FechaDesde = Hoy, FechaHasta = hasta ?? Hoy,
        Lineas = [new() { IdProducto = 1, Cantidad = 2 }, new() { IdProducto = 2, Cantidad = 1 }]
    };

    private static ResultadoCobro Cobrar(decimal cocas, decimal fernets, OfertaCombo? combo, int descuento = 0, bool alCosto = false, bool sinOfertas = false)
    {
        List<ItemCarrito> items = [];
        if (cocas > 0) items.Add(new() { IdProducto = 1, Codigo = "C1", Nombre = "Coca", Cantidad = cocas });
        if (fernets > 0) items.Add(new() { IdProducto = 2, Codigo = "C2", Nombre = "Fernet", Cantidad = fernets });
        return ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [Prod(1, "Coca", 2000), Prod(2, "Fernet", 10000)],
            Items = items,
            DescuentoPorcentaje = descuento,
            CobrarAlCosto = alCosto,
            Combos = combo == null ? [] : [combo],
            Hoy = Hoy,
            SinOfertas = sinOfertas
        });
    }

    [Fact]
    public void Combo_completo_cobra_el_precio_del_combo()
    {
        var r = Cobrar(2, 1, Combo(12000));

        Assert.Equal(12000, r.Total);
        Assert.All(r.Lineas, l => Assert.Equal("2 Coca + Fernet", l.NombreCombo));
    }

    [Fact]
    public void Combo_guarda_su_ahorro_en_las_lineas()
    {
        var r = Cobrar(2, 1, Combo(12000));

        Assert.Equal(2000, r.Lineas.Sum(l => l.AhorroCombo));
        Assert.All(r.Lineas, l => Assert.Equal(l.PrecioBase * l.Cantidad - l.AhorroCombo, l.SubTotal));
    }

    [Fact]
    public void Combo_se_repite_por_cada_set_completo()
    {
        var r = Cobrar(4, 2, Combo(12000));

        Assert.Equal(24000, r.Total);
    }

    [Fact]
    public void Lo_que_sobra_del_combo_se_cobra_a_precio_de_lista()
    {
        var r = Cobrar(3, 1, Combo(12000));

        Assert.Equal(14000, r.Total);
    }

    [Fact]
    public void Combo_incompleto_no_aplica()
    {
        var r = Cobrar(1, 1, Combo(12000));

        Assert.Equal(12000, r.Total);
        Assert.All(r.Lineas, l => Assert.Equal("", l.NombreCombo));
    }

    [Fact]
    public void Combo_mas_caro_que_la_lista_no_aplica()
    {
        var r = Cobrar(2, 1, Combo(20000));

        Assert.Equal(14000, r.Total);
    }

    [Fact]
    public void Combo_no_aplica_con_descuento_de_caja_al_costo_ni_sin_ofertas()
    {
        Assert.Equal(13300, Cobrar(2, 1, Combo(10000), descuento: 10).Total);
        Assert.Equal(7000, Cobrar(2, 1, Combo(10000), alCosto: true).Total);
        Assert.Equal(14000, Cobrar(2, 1, Combo(10000), sinOfertas: true).Total);
    }

    [Fact]
    public void Combo_fuera_de_fecha_no_aplica()
    {
        var vencido = Combo(10000);
        vencido.FechaDesde = Hoy.AddDays(-5);
        vencido.FechaHasta = Hoy.AddDays(-1);

        Assert.Equal(14000, Cobrar(2, 1, vencido).Total);
    }

    [Fact]
    public void Combo_sin_fin_sigue_vigente()
    {
        var combo = Combo(10000, DateOnly.MaxValue);

        Assert.True(combo.SinFin);
        Assert.Equal(10000, Cobrar(2, 1, combo).Total);
    }

    [Fact]
    public void Combo_descuenta_stock_de_cada_producto()
    {
        var coca = Prod(1, "Coca", 2000);
        var fernet = Prod(2, "Fernet", 10000);

        ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [coca, fernet],
            Items = [new() { IdProducto = 1, Codigo = "C1", Cantidad = 2 }, new() { IdProducto = 2, Codigo = "C2", Cantidad = 1 }],
            Combos = [Combo(12000)],
            Hoy = Hoy
        });

        Assert.Equal(48, coca.Cantidad);
        Assert.Equal(49, fernet.Cantidad);
    }

    [Fact]
    public void Validar_pide_al_menos_dos_productos_distintos()
    {
        var combo = Combo(12000);
        combo.Lineas.RemoveAt(1);

        Assert.Throws<ErrorNegocio>(() => ServicioCombos.Validar(combo, [Coca, Fernet]));
    }

    [Fact]
    public void Validar_rechaza_producto_repetido_precio_cero_y_sector()
    {
        var repetido = Combo(12000);
        repetido.Lineas[1].IdProducto = 1;
        Assert.Throws<ErrorNegocio>(() => ServicioCombos.Validar(repetido, [Coca, Fernet]));

        Assert.Throws<ErrorNegocio>(() => ServicioCombos.Validar(Combo(0), [Coca, Fernet]));

        var sector = Prod(2, "Fiambre", 100);
        sector.ProductoSector = true;
        Assert.Throws<ErrorNegocio>(() => ServicioCombos.Validar(Combo(12000), [Coca, sector]));
    }

    [Fact]
    public void Validar_acepta_un_combo_correcto()
    {
        ServicioCombos.Validar(Combo(12000), [Coca, Fernet]);
    }
}
