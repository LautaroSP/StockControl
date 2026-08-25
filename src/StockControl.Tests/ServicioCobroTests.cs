using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCobroTests
{
    private static Producto Comun(int id = 1, decimal stock = 10, decimal precio = 1500, decimal costo = 1000) =>
        new()
        {
            Id = id,
            IdLocal = 1,
            Codigo = "779",
            Nombre = "Coca",
            Cantidad = stock,
            Precio = precio,
            Costo = costo
        };

    [Fact]
    public void Cobrar_descuenta_stock_del_producto_comun()
    {
        var prod = Comun();
        var r = ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [prod],
            Items = [new ItemCarrito { IdProducto = 1, Codigo = "779", Nombre = "Coca", Cantidad = 2, PrecioUnitario = 1500 }],
            MetodoPago = "Efectivo"
        });

        Assert.Equal(8, prod.Cantidad);
        Assert.Equal(3000, r.Total);
    }

    [Fact]
    public void Sector_no_mueve_stock()
    {
        var prod = Comun();
        prod.ProductoSector = true;
        prod.Codigo = "SEC-1";
        ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [prod],
            Items = [new ItemCarrito { IdProducto = 1, Codigo = "SEC-1", Nombre = "Fiambre", Cantidad = 1, PrecioUnitario = 800 }],
            MetodoPago = "Efectivo"
        });
        Assert.Equal(10, prod.Cantidad);
    }

    [Fact]
    public void Generico_no_mueve_stock()
    {
        var prod = Comun();
        ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [prod],
            Items =
            [
                new ItemCarrito
                {
                    IdProducto = 0,
                    Codigo = "GENERIC-abc",
                    Nombre = "Suelto",
                    Cantidad = 1,
                    PrecioUnitario = 100
                }
            ],
            MetodoPago = "Efectivo"
        });
        Assert.Equal(10, prod.Cantidad);
    }

    [Fact]
    public void Stock_rigido_bloquea_si_no_alcanza()
    {
        var prod = Comun(stock: 1);
        Assert.Throws<ErrorNegocio>(() => ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [prod],
            Items = [new ItemCarrito { IdProducto = 1, Codigo = "779", Nombre = "Coca", Cantidad = 2, PrecioUnitario = 1500 }],
            StockRigido = true,
            MetodoPago = "Efectivo"
        }));
        Assert.Equal(1, prod.Cantidad);
    }

    [Fact]
    public void Descuento_sobre_margen()
    {
        var prod = Comun(precio: 1500, costo: 1000);
        var r = ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [prod],
            Items = [new ItemCarrito { IdProducto = 1, Codigo = "779", Nombre = "Coca", Cantidad = 1, PrecioUnitario = 1500 }],
            DescuentoPorcentaje = 50,
            MetodoPago = "Efectivo"
        });
        Assert.Equal(1250, r.Total);
    }

    [Fact]
    public void Cobrar_al_costo_usa_el_costo()
    {
        var prod = Comun(precio: 1500, costo: 1000);
        var r = ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [prod],
            Items = [new ItemCarrito { IdProducto = 1, Codigo = "779", Nombre = "Coca", Cantidad = 2, PrecioUnitario = 1500 }],
            CobrarAlCosto = true,
            MetodoPago = "Efectivo"
        });
        Assert.Equal(2000, r.Total);
    }

    [Fact]
    public void Cobrar_al_costo_ignora_descuento()
    {
        var prod = Comun(precio: 1500, costo: 1000);
        var r = ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [prod],
            Items = [new ItemCarrito { IdProducto = 1, Codigo = "779", Nombre = "Coca", Cantidad = 1, PrecioUnitario = 1500 }],
            CobrarAlCosto = true,
            DescuentoPorcentaje = 50,
            MetodoPago = "Efectivo"
        });
        Assert.Equal(1000, r.Total);
    }

    [Fact]
    public void Descuento_no_aplica_a_sector()
    {
        var prod = Comun(precio: 1500, costo: 1000);
        prod.ProductoSector = true;
        var r = ServicioCobro.Cobrar(new PedidoCobro
        {
            ProductosDelLocal = [prod],
            Items = [new ItemCarrito { IdProducto = 1, Codigo = "SEC-1", Nombre = "Fiambre", Cantidad = 1, PrecioUnitario = 800 }],
            DescuentoPorcentaje = 50,
            MetodoPago = "Efectivo"
        });
        Assert.Equal(800, r.Total);
    }
}
