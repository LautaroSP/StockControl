using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCatalogoProveedorTests
{
    private const decimal Factor = 1.5m;
    private const decimal Iva = 1.21m;

    private static FilaCatalogo Fila(string codigo, decimal? costo = 100m, decimal? cantidad = 10m, string nombre = "Yerba") =>
        new() { Fila = 2, Codigo = codigo, Nombre = nombre, Costo = costo, Cantidad = cantidad };

    private static RevisionCatalogo Revisar(IEnumerable<FilaCatalogo> filas, params Producto[] productos) =>
        ServicioCatalogoProveedor.Revisar(filas, productos, idProveedor: 7, Factor, Iva);

    private static ResultadoCatalogo Aplicar(IEnumerable<FilaCatalogo> filas, params Producto[] productos) =>
        ServicioCatalogoProveedor.Aplicar(filas, productos, idProveedor: 7, idLocal: 1, Factor, Iva, "dueno");

    [Fact]
    public void Fila_sin_codigo_o_con_costo_cero_queda_invalida()
    {
        var revision = Revisar(new[] { Fila(""), Fila("A1", costo: 0m), Fila("A2", costo: null) });

        Assert.Empty(revision.Validas);
        Assert.Equal(3, revision.Invalidas.Count);
    }

    [Fact]
    public void Codigo_generico_no_se_importa()
    {
        var revision = Revisar(new[] { Fila("GENERIC-1") });

        Assert.Empty(revision.Validas);
        Assert.Single(revision.Invalidas);
    }

    [Fact]
    public void Codigo_repetido_en_el_excel_queda_invalido()
    {
        var revision = Revisar(new[] { Fila("A1"), Fila("A1") });

        Assert.Single(revision.Validas);
        Assert.Equal("Código repetido en el Excel.", Assert.Single(revision.Invalidas).Motivo);
    }

    [Fact]
    public void Producto_nuevo_toma_precio_costo_por_factor_por_iva()
    {
        var fila = Assert.Single(Revisar(new[] { Fila("A1", costo: 100m) }).Validas);

        Assert.Equal(ServicioCatalogoProveedor.Crear, fila.Accion);
        Assert.Equal(181.50m, fila.Precio);
    }

    [Fact]
    public void Codigo_existente_actualiza_costo_suma_stock_y_no_duplica()
    {
        var existente = new Producto { Id = 1, Codigo = "A1", Nombre = "Yerba", Costo = 50m, Precio = 90m, Cantidad = 4m };

        var resultado = Aplicar(new[] { Fila("A1", costo: 100m, cantidad: 10m) }, existente);

        Assert.Empty(resultado.Nuevos);
        Assert.Equal(1, resultado.Actualizados);
        Assert.Equal(100m, existente.Costo);
        Assert.Equal(181.50m, existente.Precio);
        Assert.Equal(14m, existente.Cantidad);
    }

    [Fact]
    public void Producto_en_grupo_no_cambia_costo_ni_precio()
    {
        var enGrupo = new Producto { Id = 1, Codigo = "A1", Costo = 50m, Precio = 90m, IdGrupoProducto = 3 };

        Aplicar(new[] { Fila("A1", costo: 100m) }, enGrupo);

        Assert.Equal(50m, enGrupo.Costo);
        Assert.Equal(90m, enGrupo.Precio);
    }

    [Fact]
    public void Producto_con_ganancia_individual_cambia_costo_y_no_precio()
    {
        var propio = new Producto { Id = 1, Codigo = "A1", Costo = 50m, Precio = 90m, GananciaIndividual = true };

        Aplicar(new[] { Fila("A1", costo: 100m) }, propio);

        Assert.Equal(100m, propio.Costo);
        Assert.Equal(90m, propio.Precio);
    }

    [Fact]
    public void Importar_liga_los_productos_al_proveedor()
    {
        var existente = new Producto { Id = 1, Codigo = "A1", Costo = 50m, IdProveedor = 2 };

        var resultado = Aplicar(new[] { Fila("A1"), Fila("B2") }, existente);

        Assert.Equal(7, existente.IdProveedor);
        Assert.Equal(7, Assert.Single(resultado.Nuevos).IdProveedor);
    }

    [Fact]
    public void Producto_sector_no_se_importa()
    {
        var sector = new Producto { Id = 1, Codigo = "S1", ProductoSector = true };

        var revision = Revisar(new[] { Fila("S1") }, sector);

        Assert.Empty(revision.Validas);
        Assert.Single(revision.Invalidas);
    }
}
