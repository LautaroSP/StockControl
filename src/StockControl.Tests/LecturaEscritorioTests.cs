using StockControl.Dominio;

namespace StockControl.Tests;

public class LecturaEscritorioTests
{
    [Fact]
    public void AsignarLocal_pone_IdLocal_en_productos()
    {
        var lectura = new LecturaEscritorio();
        lectura.Productos.Add(new Producto { Codigo = "A", Nombre = "A" });
        lectura.AsignarLocal(7);
        Assert.All(lectura.Productos, p => Assert.Equal(7, p.IdLocal));
    }

    [Fact]
    public void Fechas_van_a_utc_para_postgres()
    {
        var lectura = new LecturaEscritorio();
        lectura.Ventas.Add(new InformeVenta
        {
            Fecha = new DateTimeOffset(2024, 6, 1, 18, 0, 0, TimeSpan.FromHours(-3))
        });
        lectura.Productos.Add(new Producto
        {
            FechaModificacion = new DateTimeOffset(2024, 6, 1, 18, 0, 0, TimeSpan.FromHours(-3))
        });
        lectura.PasarFechasAUtc();
        Assert.Equal(TimeSpan.Zero, lectura.Ventas[0].Fecha.Offset);
        Assert.Equal(TimeSpan.Zero, lectura.Productos[0].FechaModificacion!.Value.Offset);
    }

    [Fact]
    public void Codigo_duplicado_falla()
    {
        var lectura = new LecturaEscritorio();
        lectura.Productos.Add(new Producto { Codigo = "A" });
        lectura.Productos.Add(new Producto { Codigo = "A" });
        Assert.Throws<ErrorNegocio>(lectura.ValidarCodigosUnicos);
    }
}
