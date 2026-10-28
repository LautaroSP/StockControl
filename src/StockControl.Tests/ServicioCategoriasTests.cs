using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioCategoriasTests
{
    private static Categoria Categoria(int id, string nombre, int local = 1) => new() { Id = id, IdLocal = local, Nombre = nombre };

    private static Producto Producto(int id, int? categoria = null, int grupo = 0, int local = 1) =>
        new() { Id = id, IdLocal = local, Codigo = $"779{id}", Nombre = $"Producto {id}", IdCategoria = categoria, IdGrupoProducto = grupo };

    private static GrupoProductos Grupo(int id, int? categoria = null, int local = 1) =>
        new() { IdGrupoProducto = id, IdLocal = local, NombreGrupo = $"Grupo {id}", IdCategoria = categoria };

    [Fact]
    public void Alta_guarda_el_nombre_sin_espacios_de_los_costados()
    {
        var categoria = ServicioCategorias.Nueva(1, "  Bebidas ", []);

        Assert.Equal("Bebidas", categoria.Nombre);
        Assert.Equal(1, categoria.IdLocal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Nombre_vacio_se_rechaza(string? nombre)
    {
        Assert.Throws<ErrorNegocio>(() => ServicioCategorias.Nueva(1, nombre, []));
    }

    [Fact]
    public void Nombre_de_mas_de_40_caracteres_se_rechaza()
    {
        Assert.Throws<ErrorNegocio>(() => ServicioCategorias.Nueva(1, new string('a', 41), []));
    }

    [Fact]
    public void Nombre_repetido_en_el_local_se_rechaza_sin_importar_mayusculas_ni_espacios()
    {
        var existentes = new[] { Categoria(1, "Bebidas") };

        Assert.Throws<ErrorNegocio>(() => ServicioCategorias.Nueva(1, " bebidas ", existentes));
    }

    [Fact]
    public void Renombrar_con_su_mismo_nombre_se_permite()
    {
        var bebidas = Categoria(1, "Bebidas");

        ServicioCategorias.Renombrar(bebidas, "BEBIDAS", [bebidas]);

        Assert.Equal("BEBIDAS", bebidas.Nombre);
    }

    [Fact]
    public void Renombrar_con_el_nombre_de_otra_se_rechaza()
    {
        var bebidas = Categoria(1, "Bebidas");
        var limpieza = Categoria(2, "Limpieza");

        Assert.Throws<ErrorNegocio>(() => ServicioCategorias.Renombrar(limpieza, "bebidas", [bebidas, limpieza]));
    }

    [Fact]
    public void Borrar_deja_los_productos_sin_categoria_sin_tocar_los_de_otra()
    {
        var bebidas = Categoria(1, "Bebidas");
        var productos = new[] { Producto(1, categoria: 1), Producto(2, categoria: 2) };

        ServicioCategorias.Eliminar(bebidas, productos, []);

        Assert.Null(productos[0].IdCategoria);
        Assert.Equal(2, productos[1].IdCategoria);
    }

    [Fact]
    public void Borrar_deja_los_grupos_sin_categoria()
    {
        var bebidas = Categoria(1, "Bebidas");
        var grupos = new[] { Grupo(4, categoria: 1), Grupo(5, categoria: 2) };

        ServicioCategorias.Eliminar(bebidas, [], grupos);

        Assert.Null(grupos[0].IdCategoria);
        Assert.Equal(2, grupos[1].IdCategoria);
    }

    [Fact]
    public void Producto_sin_categoria_es_valido()
    {
        var producto = Producto(1, categoria: 3);

        ServicioCategorias.Asignar(producto, null, "dueno");

        Assert.Null(producto.IdCategoria);
        Assert.Equal("dueno", producto.UsuarioModificacion);
    }

    [Fact]
    public void Asignar_una_categoria_al_producto_la_guarda()
    {
        var producto = Producto(1);

        ServicioCategorias.Asignar(producto, Categoria(3, "Almacén"));

        Assert.Equal(3, producto.IdCategoria);
    }

    [Fact]
    public void No_se_asigna_al_producto_una_categoria_de_otro_local()
    {
        var producto = Producto(1, local: 1);

        Assert.Throws<ErrorNegocio>(() => ServicioCategorias.Asignar(producto, Categoria(3, "Almacén", local: 2)));
        Assert.Null(producto.IdCategoria);
    }

    [Fact]
    public void No_se_asigna_al_grupo_una_categoria_de_otro_local()
    {
        var grupo = Grupo(4, local: 1);

        Assert.Throws<ErrorNegocio>(() => ServicioCategorias.AsignarAGrupo(grupo, Categoria(3, "Almacén", local: 2)));
        Assert.Null(grupo.IdCategoria);
    }

    [Fact]
    public void La_categoria_propia_domina_a_la_del_grupo()
    {
        Assert.Equal(2, ServicioCategorias.Efectiva(Producto(1, categoria: 2, grupo: 4), Grupo(4, categoria: 1)));
    }

    [Fact]
    public void Sin_categoria_propia_hereda_la_del_grupo()
    {
        Assert.Equal(1, ServicioCategorias.Efectiva(Producto(1, grupo: 4), Grupo(4, categoria: 1)));
    }

    [Fact]
    public void Sin_categoria_propia_ni_del_grupo_queda_sin_categoria()
    {
        Assert.Null(ServicioCategorias.Efectiva(Producto(1, grupo: 4), Grupo(4)));
        Assert.Null(ServicioCategorias.Efectiva(Producto(2), null));
    }

    [Fact]
    public void No_hereda_la_categoria_de_un_grupo_que_no_es_el_suyo()
    {
        Assert.Null(ServicioCategorias.Efectiva(Producto(1, grupo: 5), Grupo(4, categoria: 1)));
    }

    [Fact]
    public void Contar_suma_los_productos_que_heredan_del_grupo()
    {
        var productos = new[] { Producto(1, categoria: 1), Producto(2, grupo: 4), Producto(3, grupo: 4, categoria: 2), Producto(4) };

        var conteo = ServicioCategorias.ContarProductos(productos, [Grupo(4, categoria: 1)]);

        Assert.Equal(2, conteo[1]);
        Assert.Equal(1, conteo[2]);
        Assert.Equal(2, conteo.Count);
    }
}
