using StockControl.Dominio;

namespace StockControl.Tests;

public class ServicioPlantillasCategoriaTests
{
    [Fact]
    public void Hay_plantillas_de_almacen_y_refrigeracion()
    {
        Assert.Contains(ServicioPlantillasCategoria.Todas, p => p.Clave == "almacen");
        Assert.Contains(ServicioPlantillasCategoria.Todas, p => p.Clave == "refrigeracion");
        Assert.NotNull(ServicioPlantillasCategoria.PorClave("almacen"));
        Assert.Null(ServicioPlantillasCategoria.PorClave("no-existe"));
    }

    [Fact]
    public void Aplicar_crea_raices_e_hijos_sin_duplicar()
    {
        var plantilla = ServicioPlantillasCategoria.PorClave("almacen")!;
        var delLocal = new List<Categoria>();

        var raices = ServicioPlantillasCategoria.AplicarRaices(1, plantilla, delLocal);
        Assert.NotEmpty(raices);
        Assert.All(raices, r => Assert.Null(r.IdPadre));

        for (var i = 0; i < raices.Count; i++)
            raices[i].Id = i + 1;

        var otraVez = ServicioPlantillasCategoria.AplicarRaices(1, plantilla, delLocal);
        Assert.Empty(otraVez);

        var hijos = ServicioPlantillasCategoria.AplicarHijos(1, plantilla, delLocal);
        Assert.NotEmpty(hijos);
        Assert.All(hijos, h => Assert.True(h.IdPadre > 0));
        Assert.Contains(hijos, h => h.Nombre == "Gaseosas");
    }

    [Fact]
    public void Expandir_padre_incluye_hijos()
    {
        var cats = new[]
        {
            new Categoria { Id = 1, IdLocal = 1, Nombre = "Bebidas" },
            new Categoria { Id = 2, IdLocal = 1, Nombre = "Gaseosas", IdPadre = 1 },
            new Categoria { Id = 3, IdLocal = 1, Nombre = "Agua", IdPadre = 1 }
        };

        var ids = ServicioCategorias.ExpandirConHijos([1], cats);

        Assert.Equal(3, ids.Count);
        Assert.Contains(2, ids);
        Assert.Contains(3, ids);
    }

    [Fact]
    public void Contar_con_padres_suma_hijos()
    {
        var cats = new[]
        {
            new Categoria { Id = 1, Nombre = "Bebidas" },
            new Categoria { Id = 2, Nombre = "Gaseosas", IdPadre = 1 }
        };
        var propios = new Dictionary<int, int> { [2] = 5 };

        var total = ServicioCategorias.ContarConPadres(propios, cats);

        Assert.Equal(5, total[1]);
        Assert.Equal(5, total[2]);
    }
}
