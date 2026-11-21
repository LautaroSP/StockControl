using StockControl.Api.Tienda;
using StockControl.Dominio;

namespace StockControl.Tests;

public class AlmacenImagenesTests
{
    [Fact]
    public async Task Reponer_escribe_el_archivo_que_nombra_la_fila()
    {
        var raiz = Directory.CreateTempSubdirectory("sc-img");
        try
        {
            var almacen = new AlmacenImagenes(raiz.FullName);
            var imagen = ImagenOff("d18706839ade42279ba6ae10e9c5a4ec.webp");
            var datos = new byte[] { 1, 2, 3 };

            Assert.Null(almacen.Ruta(imagen));
            var ruta = await almacen.Reponer(imagen, datos, CancellationToken.None);

            Assert.NotNull(ruta);
            Assert.Equal(ruta, almacen.Ruta(imagen));
            Assert.Equal(datos, await File.ReadAllBytesAsync(ruta!));
        }
        finally
        {
            raiz.Delete(true);
        }
    }

    [Fact]
    public async Task Reponer_no_escribe_si_el_nombre_no_es_del_servidor()
    {
        var raiz = Directory.CreateTempSubdirectory("sc-img");
        try
        {
            var almacen = new AlmacenImagenes(raiz.FullName);
            var imagen = ImagenOff("../secreto.webp");

            Assert.Null(await almacen.Reponer(imagen, [1, 2, 3], CancellationToken.None));
            Assert.False(File.Exists(Path.Combine(raiz.FullName, "secreto.webp")));
        }
        finally
        {
            raiz.Delete(true);
        }
    }

    private static ImagenProducto ImagenOff(string archivo) => new()
    {
        Id = 1,
        IdLocal = 1,
        IdProducto = 1,
        Orden = 1,
        Archivo = archivo,
        Origen = ServicioImagenesTienda.OrigenOpenFoodFacts,
        TipoContenido = ServicioImagenesTienda.WebP
    };
}
