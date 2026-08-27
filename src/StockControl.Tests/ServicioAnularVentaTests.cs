using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioAnularVentaTests
{
    private static Producto Comun(int id = 1, decimal stock = 8) =>
        new()
        {
            Id = id,
            IdLocal = 1,
            Codigo = "779",
            Nombre = "Coca",
            Cantidad = stock,
            Precio = 1500,
            Costo = 1000
        };

    [Fact]
    public void Anular_devuelve_stock_del_comun()
    {
        var prod = Comun();
        var venta = new InformeVenta
        {
            Detalles =
            [
                new InformeVentaDetalle { IdProducto = 1, Codigo = "779", Nombre = "Coca", Cantidad = 2, Precio = 1500, SubTotal = 3000 }
            ]
        };
        ServicioAnularVenta.Anular(venta, [prod]);
        Assert.Equal(10, prod.Cantidad);
    }

    [Fact]
    public void Anular_no_mueve_stock_de_sector()
    {
        var prod = Comun();
        prod.ProductoSector = true;
        var venta = new InformeVenta
        {
            Detalles =
            [
                new InformeVentaDetalle { IdProducto = 1, Codigo = "SEC-1", Nombre = "Fiambre", Cantidad = 1, Precio = 800, SubTotal = 800 }
            ]
        };
        ServicioAnularVenta.Anular(venta, [prod]);
        Assert.Equal(8, prod.Cantidad);
    }

    [Fact]
    public void Anular_no_mueve_stock_de_generico()
    {
        var prod = Comun();
        var venta = new InformeVenta
        {
            Detalles =
            [
                new InformeVentaDetalle { IdProducto = 0, Codigo = "GENERIC-abc", Nombre = "Suelto", Cantidad = 1, Precio = 100, SubTotal = 100 }
            ]
        };
        ServicioAnularVenta.Anular(venta, [prod]);
        Assert.Equal(8, prod.Cantidad);
    }

    [Fact]
    public void Anular_falla_si_la_caja_ya_cerro()
    {
        var prod = Comun();
        var venta = new InformeVenta
        {
            NroCaja = 1,
            IdCierre = 4,
            Detalles =
            [
                new InformeVentaDetalle { IdProducto = 1, Codigo = "779", Nombre = "Coca", Cantidad = 2 }
            ]
        };
        Assert.Throws<ErrorNegocio>(() => ServicioAnularVenta.Anular(venta, [prod]));
        Assert.Equal(8, prod.Cantidad);
    }
}
