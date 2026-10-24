using System.Globalization;
using ClosedXML.Excel;
using StockControl.Dominio;

namespace StockControl.Api.Importacion;

public static class ExcelProveedor
{
    public const string TipoContenido = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const int MaxBytes = 2 * 1024 * 1024;
    public const int MaxFilas = 2000;

    private static readonly string[] Encabezados = { "Codigo", "Nombre de producto", "Costo", "Cantidad" };
    private static readonly CultureInfo Ar = new("es-AR");

    public static byte[] Plantilla()
    {
        using var libro = new XLWorkbook();
        var hoja = libro.AddWorksheet("Productos");
        for (var i = 0; i < Encabezados.Length; i++)
            hoja.Cell(1, i + 1).Value = Encabezados[i];
        hoja.Row(1).Style.Font.Bold = true;
        hoja.Column(1).Style.NumberFormat.Format = "@";
        hoja.Columns().AdjustToContents();
        return Guardar(libro);
    }

    public static List<FilaCatalogo> LeerCatalogo(byte[] archivo)
    {
        if (archivo.Length > MaxBytes)
            throw new ErrorNegocio("El Excel no puede pesar más de 2 MB.");
        using var libro = Abrir(archivo);
        var hoja = libro.Worksheets.First();
        ValidarEncabezados(hoja);

        var filas = new List<FilaCatalogo>();
        var ultima = hoja.LastRowUsed()?.RowNumber() ?? 1;
        for (var nro = 2; nro <= ultima; nro++)
        {
            var fila = hoja.Row(nro);
            if (fila.Cells(1, 4).All(c => c.IsEmpty())) continue;
            if (filas.Count == MaxFilas)
                throw new ErrorNegocio($"El Excel no puede tener más de {MaxFilas} productos.");
            filas.Add(new FilaCatalogo
            {
                Fila = nro,
                Codigo = Texto(fila.Cell(1)),
                Nombre = Texto(fila.Cell(2)),
                Costo = Numero(fila.Cell(3), vacio: null),
                Cantidad = Numero(fila.Cell(4), vacio: 0m)
            });
        }
        return filas;
    }

    public static byte[] Orden(string nombreLocal, Proveedor proveedor, OrdenCompra orden)
    {
        using var libro = new XLWorkbook();
        var hoja = libro.AddWorksheet("Orden");
        hoja.Cell(1, 1).Value = "Local";
        hoja.Cell(1, 2).Value = nombreLocal;
        hoja.Cell(2, 1).Value = "Proveedor";
        hoja.Cell(2, 2).Value = proveedor.Nombre;
        hoja.Cell(3, 1).Value = "Fecha";
        hoja.Cell(3, 2).Value = TimeZoneInfo.ConvertTime(orden.Fecha, DiaArgentina.Zona).ToString("dd/MM/yyyy HH:mm", Ar);
        hoja.Range(1, 1, 3, 1).Style.Font.Bold = true;

        hoja.Cell(5, 1).Value = "Producto";
        hoja.Cell(5, 2).Value = "Cantidad";
        hoja.Row(5).Style.Font.Bold = true;

        var nro = 6;
        foreach (var l in orden.Lineas)
        {
            hoja.Cell(nro, 1).Value = ServicioOrdenCompra.Descripcion(l);
            hoja.Cell(nro, 2).Value = l.Cantidad;
            nro++;
        }
        hoja.Columns().AdjustToContents();
        return Guardar(libro);
    }

    private static XLWorkbook Abrir(byte[] archivo)
    {
        try
        {
            return new XLWorkbook(new MemoryStream(archivo));
        }
        catch (Exception)
        {
            throw new ErrorNegocio("El archivo no es un Excel válido (.xlsx).");
        }
    }

    private static void ValidarEncabezados(IXLWorksheet hoja)
    {
        for (var i = 0; i < Encabezados.Length; i++)
        {
            if (!string.Equals(Texto(hoja.Cell(1, i + 1)), Encabezados[i], StringComparison.OrdinalIgnoreCase))
                throw new ErrorNegocio("Usá la plantilla: columnas Codigo, Nombre de producto, Costo, Cantidad.");
        }
    }

    private static string Texto(IXLCell celda)
    {
        var valor = celda.Value;
        var texto = valor.IsNumber
            ? valor.GetNumber().ToString("0.#########", CultureInfo.InvariantCulture)
            : celda.GetFormattedString();
        return texto.Trim();
    }

    private static decimal? Numero(IXLCell celda, decimal? vacio)
    {
        var valor = celda.Value;
        if (valor.IsBlank) return vacio;
        if (valor.IsNumber) return Math.Round((decimal)valor.GetNumber(), 3);
        var texto = celda.GetString().Trim().Replace("$", "").Trim();
        if (texto.Length == 0) return vacio;
        var cultura = texto.Contains(',') ? Ar : CultureInfo.InvariantCulture;
        return decimal.TryParse(texto, NumberStyles.Number, cultura, out var numero) ? numero : null;
    }

    private static byte[] Guardar(XLWorkbook libro)
    {
        using var salida = new MemoryStream();
        libro.SaveAs(salida);
        return salida.ToArray();
    }
}
