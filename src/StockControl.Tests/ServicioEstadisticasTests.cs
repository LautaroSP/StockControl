using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioEstadisticasTests
{
    private static Local CrearLocal(int id, string nombre) =>
        new() { IdLocal = id, Nombre = nombre };

    private static Producto CrearProducto(
        int id,
        int idLocal,
        string codigo,
        string nombre,
        decimal cantidad,
        decimal precio,
        decimal costo = 0,
        bool sector = false,
        int idGrupo = 0) =>
        new()
        {
            Id = id,
            IdLocal = idLocal,
            Codigo = codigo,
            Nombre = nombre,
            Cantidad = cantidad,
            Precio = precio,
            Costo = costo,
            ProductoSector = sector,
            IdGrupoProducto = idGrupo
        };

    private static InformeVenta CrearVenta(
        int id,
        int idLocal,
        decimal total,
        string metodoPago = "Efectivo",
        int nroCaja = 1,
        List<InformeVentaDetalle>? detalles = null,
        List<PagoVenta>? pagos = null) =>
        new()
        {
            IdInformeVenta = id,
            IdLocal = idLocal,
            Total = total,
            Subtotal = total,
            MetodoPago = metodoPago,
            NroCaja = nroCaja,
            Detalles = detalles ?? new List<InformeVentaDetalle>(),
            Pagos = pagos ?? new List<PagoVenta>()
        };

    [Fact]
    public void Estadisticas_calcula_totales_tickets_y_promedio()
    {
        var ventas = new List<InformeVenta>
        {
            CrearVenta(1, 1, 1000),
            CrearVenta(2, 1, 2000),
            CrearVenta(3, 1, 3000)
        };

        var res = ServicioEstadisticas.Calcular(ventas, [], [], [], [CrearLocal(1, "Central")]);

        Assert.Equal(6000, res.TotalVendido);
        Assert.Equal(3, res.CantidadTickets);
        Assert.Equal(2000, res.TicketPromedio);
    }

    [Fact]
    public void Estadisticas_top_productos_ignora_genericos()
    {
        var p1 = CrearProducto(1, 1, "COCA", "Coca Cola", 10, 500);
        var p2 = CrearProducto(2, 1, "ALFAJOR", "Alfajor", 20, 200);

        var v1 = CrearVenta(1, 1, 900, detalles: new List<InformeVentaDetalle>
        {
            new() { Codigo = "GENERIC-123", Nombre = "Varios", Cantidad = 10, SubTotal = 5000, Precio = 500 },
            new() { Codigo = "COCA", Nombre = "Coca Cola", Cantidad = 2, SubTotal = 1000, Precio = 500 },
            new() { Codigo = "ALFAJOR", Nombre = "Alfajor", Cantidad = 5, SubTotal = 1000, Precio = 200 }
        });

        var res = ServicioEstadisticas.Calcular([v1], [], [p1, p2], [], [CrearLocal(1, "Central")]);

        Assert.DoesNotContain(res.TopPorUnidades, x => x.Codigo.StartsWith("GENERIC-"));
        Assert.DoesNotContain(res.TopPorDinero, x => x.Codigo.StartsWith("GENERIC-"));
        Assert.Equal("ALFAJOR", res.MasVendidoUnidades?.Codigo);
        Assert.Equal(5, res.MasVendidoUnidades?.Cantidad);
    }

    [Fact]
    public void Estadisticas_sector_cuenta_por_dinero_y_no_por_unidades()
    {
        var prodSector = CrearProducto(1, 1, "FIAMBRE", "Fiambre x peso", 0, 1000, sector: true);
        var prodComun = CrearProducto(2, 1, "PAN", "Pan Lactal", 15, 300);

        var v1 = CrearVenta(1, 1, 1300, detalles: new List<InformeVentaDetalle>
        {
            new() { Codigo = "FIAMBRE", Nombre = "Fiambre x peso", Cantidad = 1, SubTotal = 1000, Precio = 1000 },
            new() { Codigo = "PAN", Nombre = "Pan Lactal", Cantidad = 2, SubTotal = 600, Precio = 300 }
        });

        var res = ServicioEstadisticas.Calcular([v1], [], [prodSector, prodComun], [], [CrearLocal(1, "Central")]);

        // En top unidades no debe figurar Fiambre porque es sector
        Assert.DoesNotContain(res.TopPorUnidades, x => x.Codigo == "FIAMBRE");
        Assert.Equal("PAN", res.MasVendidoUnidades?.Codigo);

        // En top dinero sí debe figurar y liderar
        Assert.Equal("FIAMBRE", res.MasVendidoDinero?.Codigo);
        Assert.Equal(1000, res.MasVendidoDinero?.Total);
    }

    [Fact]
    public void Estadisticas_calcula_margen_solo_si_hay_costo()
    {
        // Producto 1: Precio 500, Costo 300 -> Margen 200 * 2 = 400
        // Producto 2: Precio 200, Costo null / 0 -> Sin costo, no entra en el margen
        var v1 = CrearVenta(1, 1, 1200, detalles: new List<InformeVentaDetalle>
        {
            new() { Codigo = "P1", Nombre = "Prod 1", Cantidad = 2, Precio = 500, Costo = 300, SubTotal = 1000 },
            new() { Codigo = "P2", Nombre = "Prod 2", Cantidad = 1, Precio = 200, Costo = null, SubTotal = 200 }
        });

        var res = ServicioEstadisticas.Calcular([v1], [], [], [], [CrearLocal(1, "Central")]);

        // Margen estimado: 1000 - (300 * 2) = 400
        Assert.Equal(400, res.MargenEstimado);
        // Porcentaje sobre los ítems con costo (400 / 1000 = 40%)
        Assert.Equal(40, res.PorcentajeMargen);
    }

    [Fact]
    public void Ganancia_del_dia_de_sector_y_generico_usa_el_factor_del_local()
    {
        var sector = CrearProducto(1, 1, "FIAMBRE", "Fiambre", 0, 0, sector: true);
        var hoy = new List<InformeVenta>
        {
            CrearVenta(1, 1, 2500, detalles: new List<InformeVentaDetalle>
            {
                new() { Codigo = "FIAMBRE", Nombre = "Fiambre", Cantidad = 1, Precio = 1500, SubTotal = 1500 },
                new() { Codigo = "GENERIC-1", Nombre = "Varios", Cantidad = 1, Precio = 1000, SubTotal = 1000 }
            })
        };
        var factores = new Dictionary<int, FactoresGananciaLibre>
        {
            [1] = new(Sector: 1.5m, Generico: 2m)
        };

        var res = ServicioEstadisticas.Calcular(hoy, hoy, [sector], [], [CrearLocal(1, "Central")], factoresPorLocal: factores);

        // 1500 − 1500/1.5 = 500; 1000 − 1000/2 = 500
        Assert.Equal(1000, res.GananciaDelDia);
        Assert.Equal(1000, res.MargenEstimado);
    }

    [Fact]
    public void Generico_sin_factor_propio_usa_el_factor_de_ganancia()
    {
        var venta = CrearVenta(1, 1, 1000, detalles: new List<InformeVentaDetalle>
        {
            new() { Codigo = "GENERIC-1", Nombre = "Varios", Cantidad = 1, Precio = 1000, SubTotal = 1000 }
        });
        var factores = new Dictionary<int, FactoresGananciaLibre>
        {
            [1] = new(Sector: 1, Generico: 1, Comun: 1.5m)
        };

        var res = ServicioEstadisticas.Calcular([venta], [], [], [], [CrearLocal(1, "Central")], factoresPorLocal: factores);

        Assert.Equal(333.33m, res.MargenEstimado);
        Assert.Equal(33.33m, res.PorcentajeMargen);
    }

    [Fact]
    public void Sector_con_costo_usa_el_costo_y_no_el_factor()
    {
        var sector = CrearProducto(1, 1, "FIAMBRE", "Fiambre", 0, 0, sector: true);
        var venta = CrearVenta(1, 1, 1500, detalles: new List<InformeVentaDetalle>
        {
            new() { Codigo = "FIAMBRE", Nombre = "Fiambre", Cantidad = 1, Precio = 1500, Costo = 900, SubTotal = 1500 }
        });
        var factores = new Dictionary<int, FactoresGananciaLibre> { [1] = new(3, 3) };

        var res = ServicioEstadisticas.Calcular([venta], [venta], [sector], [], [CrearLocal(1, "Central")], factoresPorLocal: factores);

        Assert.Equal(600, res.GananciaDelDia);
    }

    [Fact]
    public void Factor_en_uno_no_estima_ganancia_de_precio_libre()
    {
        var sector = CrearProducto(1, 1, "FIAMBRE", "Fiambre", 0, 0, sector: true);
        var venta = CrearVenta(1, 1, 1500, detalles: new List<InformeVentaDetalle>
        {
            new() { Codigo = "FIAMBRE", Nombre = "Fiambre", Cantidad = 1, Precio = 1500, SubTotal = 1500 }
        });

        var res = ServicioEstadisticas.Calcular([venta], [venta], [sector], [], [CrearLocal(1, "Central")]);

        Assert.Equal(0, res.GananciaDelDia);
        Assert.Equal(0, res.MargenEstimado);
    }

    [Fact]
    public void Estadisticas_mix_de_pagos_calcula_totales_y_porcentajes()
    {
        var v1 = CrearVenta(1, 1, 1000, pagos: new List<PagoVenta>
        {
            new() { DescripcionMetodoPago = "Efectivo", Importe = 600 },
            new() { DescripcionMetodoPago = "Mercado Pago", Importe = 400 }
        });
        var v2 = CrearVenta(2, 1, 1000, metodoPago: "Efectivo"); // sin lista pagos, usa MetodoPago

        var res = ServicioEstadisticas.Calcular([v1, v2], [], [], [], [CrearLocal(1, "Central")]);

        var efectivo = res.MixMediosPago.FirstOrDefault(m => m.Descripcion == "Efectivo");
        var mp = res.MixMediosPago.FirstOrDefault(m => m.Descripcion == "Mercado Pago");

        Assert.NotNull(efectivo);
        Assert.NotNull(mp);
        Assert.Equal(1600, efectivo.Total);
        Assert.Equal(80, efectivo.Porcentaje);
        Assert.Equal(400, mp.Total);
        Assert.Equal(20, mp.Porcentaje);
    }

    [Fact]
    public void Estadisticas_ventas_por_grupo_agrupa_correctamente()
    {
        var grupos = new List<GrupoProductos>
        {
            new() { IdGrupoProducto = 1, IdLocal = 1, NombreGrupo = "Bebidas" },
            new() { IdGrupoProducto = 2, IdLocal = 1, NombreGrupo = "Golosinas" }
        };

        var pBebida = CrearProducto(1, 1, "COCA", "Coca Cola", 10, 500, idGrupo: 1);
        var pGolosina = CrearProducto(2, 1, "CHOCO", "Chocolate", 10, 300, idGrupo: 2);
        var pSuelto = CrearProducto(3, 1, "SUELTO", "Sin Grupo", 10, 200, idGrupo: 0);

        var v1 = CrearVenta(1, 1, 1000, detalles: new List<InformeVentaDetalle>
        {
            new() { Codigo = "COCA", Nombre = "Coca Cola", Cantidad = 1, SubTotal = 500 },
            new() { Codigo = "CHOCO", Nombre = "Chocolate", Cantidad = 1, SubTotal = 300 },
            new() { Codigo = "SUELTO", Nombre = "Sin Grupo", Cantidad = 1, SubTotal = 200 }
        });

        var res = ServicioEstadisticas.Calcular([v1], [], [pBebida, pGolosina, pSuelto], grupos, [CrearLocal(1, "Central")]);

        var gBebidas = res.VentasPorGrupo.FirstOrDefault(g => g.NombreGrupo == "Bebidas");
        var gGolosinas = res.VentasPorGrupo.FirstOrDefault(g => g.NombreGrupo == "Golosinas");
        var gSinGrupo = res.VentasPorGrupo.FirstOrDefault(g => g.NombreGrupo == "Sin grupo");

        Assert.NotNull(gBebidas);
        Assert.NotNull(gGolosinas);
        Assert.NotNull(gSinGrupo);
        Assert.Equal(500, gBebidas.Total);
        Assert.Equal(50, gBebidas.Porcentaje);
        Assert.Equal(300, gGolosinas.Total);
        Assert.Equal(30, gGolosinas.Porcentaje);
        Assert.Equal(200, gSinGrupo.Total);
        Assert.Equal(20, gSinGrupo.Porcentaje);
    }

    [Fact]
    public void Estadisticas_comparacion_sucursales_separa_por_local()
    {
        var l1 = CrearLocal(1, "Sucursal Almagro");
        var l2 = CrearLocal(2, "Sucursal Caballito");

        var ventas = new List<InformeVenta>
        {
            CrearVenta(1, 1, 1000),
            CrearVenta(2, 1, 2000),
            CrearVenta(3, 2, 500)
        };

        var res = ServicioEstadisticas.Calcular(ventas, [], [], [], [l1, l2]);

        Assert.Equal(2, res.ComparativaSucursales.Count);
        var alma = res.ComparativaSucursales.First(s => s.IdLocal == 1);
        var cab = res.ComparativaSucursales.First(s => s.IdLocal == 2);

        Assert.Equal("Sucursal Almagro", alma.NombreLocal);
        Assert.Equal(3000, alma.TotalVendido);
        Assert.Equal(2, alma.CantidadTickets);
        Assert.Equal(1500, alma.TicketPromedio);

        Assert.Equal("Sucursal Caballito", cab.NombreLocal);
        Assert.Equal(500, cab.TotalVendido);
        Assert.Equal(1, cab.CantidadTickets);
        Assert.Equal(500, cab.TicketPromedio);
    }

    [Fact]
    public void Estadisticas_stock_bajo_filtra_por_umbral_y_excluye_sector_y_genericos()
    {
        var pNormalBajo = CrearProducto(1, 1, "YERBA", "Yerba", cantidad: 3, precio: 800); // <= 5
        var pNormalOk = CrearProducto(2, 1, "AZUCAR", "Azucar", cantidad: 10, precio: 400); // > 5
        var pSectorBajo = CrearProducto(3, 1, "QUESO", "Queso x kg", cantidad: 1, precio: 1200, sector: true); // sector no alerta stock
        var pGenerico = CrearProducto(4, 1, "GENERIC-01", "Generico", cantidad: 0, precio: 100);

        var umbrales = new Dictionary<int, decimal> { [1] = 5 };

        var res = ServicioEstadisticas.Calcular(
            [],
            [],
            [pNormalBajo, pNormalOk, pSectorBajo, pGenerico],
            [],
            [CrearLocal(1, "Central")],
            umbrales);

        Assert.Single(res.StockBajo);
        Assert.Equal("YERBA", res.StockBajo[0].Codigo);
        Assert.Equal(3, res.StockBajo[0].Cantidad);
        Assert.Equal(5, res.StockBajo[0].Umbral);
    }

    [Fact]
    public void Estadisticas_ventas_del_dia_agrupa_todas_las_cajas_y_ordena_por_cantidad()
    {
        var pAgua = CrearProducto(1, 1, "AGUA", "Agua Mineral", cantidad: 45, precio: 250);
        var pGalletitas = CrearProducto(2, 1, "GALL", "Galletitas", cantidad: 12, precio: 400);
        var pCaramelo = CrearProducto(3, 1, "CARA", "Caramelo", cantidad: 80, precio: 50);

        // Ventas de hoy en diferentes cajas (Caja 1 y Caja 2)
        var vCaja1 = CrearVenta(1, 1, 1000, nroCaja: 1, detalles: new List<InformeVentaDetalle>
        {
            new() { Codigo = "AGUA", Nombre = "Agua Mineral", Cantidad = 3, SubTotal = 750 },
            new() { Codigo = "GALL", Nombre = "Galletitas", Cantidad = 1, SubTotal = 400 }
        });

        var vCaja2 = CrearVenta(2, 1, 2000, nroCaja: 2, detalles: new List<InformeVentaDetalle>
        {
            new() { Codigo = "AGUA", Nombre = "Agua Mineral", Cantidad = 7, SubTotal = 1750 },
            new() { Codigo = "CARA", Nombre = "Caramelo", Cantidad = 15, SubTotal = 750 }
        });

        var res = ServicioEstadisticas.Calcular(
            ventasPeriodo: [],
            ventasHoy: [vCaja1, vCaja2],
            productos: [pAgua, pGalletitas, pCaramelo],
            grupos: [],
            locales: [CrearLocal(1, "Central")]);

        Assert.Equal(3, res.VentasDelDia.Count);

        // Orden descendente por cantidad: Caramelo (15), Agua (10 = 3+7), Galletitas (1)
        Assert.Equal("CARA", res.VentasDelDia[0].Codigo);
        Assert.Equal(15, res.VentasDelDia[0].CantidadVentas);
        Assert.Equal(80, res.VentasDelDia[0].StockRestante);

        Assert.Equal("AGUA", res.VentasDelDia[1].Codigo);
        Assert.Equal(10, res.VentasDelDia[1].CantidadVentas);
        Assert.Equal(45, res.VentasDelDia[1].StockRestante);

        Assert.Equal("GALL", res.VentasDelDia[2].Codigo);
        Assert.Equal(1, res.VentasDelDia[2].CantidadVentas);
        Assert.Equal(12, res.VentasDelDia[2].StockRestante);
    }

    private static List<InformeVentaDetalle> Fiado(string codigo, decimal cantidad, decimal subtotal) =>
        ServicioCuentas.UnidadesFiadas([new CuentaPendiente
        {
            IdLocal = 1,
            Lineas = [new LineaCuenta { IdLocal = 1, IdProducto = 1, Codigo = codigo, Nombre = codigo, Cantidad = cantidad, Precio = subtotal / cantidad, Costo = 100, SubTotal = subtotal }]
        }]);

    [Fact]
    public void Fiado_sin_pagar_no_suma_total_ticket_ni_ganancia()
    {
        var venta = CrearVenta(1, 1, 1000, detalles: [new() { Codigo = "PAN", Nombre = "Pan", Cantidad = 1, Costo = 600, SubTotal = 1000 }]);

        var res = ServicioEstadisticas.Calcular([venta], [], [], [], [CrearLocal(1, "Central")], fiadoPeriodo: Fiado("LECHE", 3, 3000));

        Assert.Equal(1000, res.TotalVendido);
        Assert.Equal(1, res.CantidadTickets);
        Assert.Equal(400, res.MargenEstimado);
    }

    [Fact]
    public void Fiado_suma_unidades_en_el_top_sin_plata()
    {
        var res = ServicioEstadisticas.Calcular([], [], [], [], [CrearLocal(1, "Central")], fiadoPeriodo: Fiado("LECHE", 3, 3000));

        var leche = Assert.Single(res.TopPorUnidades);
        Assert.Equal(3, leche.Cantidad);
        Assert.Equal(0, leche.Total);
        Assert.Empty(res.TopPorDinero);
    }

    [Fact]
    public void Fiado_de_hoy_suma_en_ventas_del_dia()
    {
        var res = ServicioEstadisticas.Calcular([], [], [], [], [CrearLocal(1, "Central")], fiadoHoy: Fiado("LECHE", 2, 2000));

        Assert.Equal(2, Assert.Single(res.VentasDelDia).CantidadVentas);
    }

    [Fact]
    public void Pago_de_cuenta_suma_plata_pero_no_aparece_en_el_top()
    {
        var pago = CrearVenta(1, 1, 1500, detalles: [new() { Codigo = "CUENTA-9", Nombre = "Pago cuenta de María", Cantidad = 1, Costo = 1000, SubTotal = 1500 }]);

        var res = ServicioEstadisticas.Calcular([pago], [pago], [], [], [CrearLocal(1, "Central")]);

        Assert.Equal(1500, res.TotalVendido);
        Assert.Equal(500, res.MargenEstimado);
        Assert.Empty(res.TopPorUnidades);
        Assert.Empty(res.TopPorDinero);
        Assert.Empty(res.VentasDelDia);
        Assert.Empty(res.VentasPorGrupo);
    }
}
