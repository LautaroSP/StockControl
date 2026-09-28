using ClosedXML.Excel;
using StockControl.Api.Importacion;
using StockControl.Dominio;

namespace StockControl.Tests;

public class ExcelProveedorTests
{
    [Fact]
    public void Plantilla_completada_se_lee_como_catalogo()
    {
        using var libro = new XLWorkbook(new MemoryStream(ExcelProveedor.Plantilla()));
        var hoja = libro.Worksheet(1);
        hoja.Cell(2, 1).Value = 7790001234567;
        hoja.Cell(2, 2).Value = "Yerba 1kg";
        hoja.Cell(2, 3).Value = 1500.5;
        hoja.Cell(2, 4).Value = 12;
        hoja.Cell(4, 1).Value = "B2";
        hoja.Cell(4, 2).Value = "Azúcar";
        hoja.Cell(4, 3).Value = "1.200,50";
        using var salida = new MemoryStream();
        libro.SaveAs(salida);

        var filas = ExcelProveedor.LeerCatalogo(salida.ToArray());

        Assert.Equal(2, filas.Count);
        Assert.Equal("7790001234567", filas[0].Codigo);
        Assert.Equal(1500.5m, filas[0].Costo);
        Assert.Equal(12m, filas[0].Cantidad);
        Assert.Equal(4, filas[1].Fila);
        Assert.Equal(1200.50m, filas[1].Costo);
        Assert.Equal(0m, filas[1].Cantidad);
    }

    [Fact]
    public void Excel_sin_las_columnas_de_la_plantilla_da_error()
    {
        using var libro = new XLWorkbook();
        libro.AddWorksheet("Otra").Cell(1, 1).Value = "Producto";
        using var salida = new MemoryStream();
        libro.SaveAs(salida);

        Assert.Throws<ErrorNegocio>(() => ExcelProveedor.LeerCatalogo(salida.ToArray()));
    }
}
