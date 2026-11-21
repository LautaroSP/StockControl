namespace StockControl.Dominio;

public sealed record NodoPlantilla(string Nombre, IReadOnlyList<NodoPlantilla> Hijos)
{
    public NodoPlantilla(string nombre, params string[] hijos)
        : this(nombre, hijos.Length == 0 ? Array.Empty<NodoPlantilla>() : hijos.Select(h => new NodoPlantilla(h)).ToList()) { }
}

public sealed record PlantillaCategoria(string Clave, string Titulo, string Descripcion, IReadOnlyList<NodoPlantilla> Raices);

/// <summary>Sugerencias de categorías según el rubro del local. Se pueden editar después.</summary>
public static class ServicioPlantillasCategoria
{
    public static readonly IReadOnlyList<PlantillaCategoria> Todas =
    [
        new("almacen", "Almacén / kiosco", "Bebidas, lácteos, galletitas, almacén, snacks y limpieza.",
        [
            new("Bebidas", "Gaseosas", "Agua", "Gaseosa sin azúcar", "Agua con gas", "Jugos"),
            new("Lácteos", "Leches", "Yogurt", "Manteca", "Crema de leche", "Quesos", "Crema"),
            new("Galletitas"),
            new("Almacén", "Aceites", "Arroz y pastas", "Conservas", "Azúcar y endulzantes", "Harinas"),
            new("Snacks", "Papas fritas", "Maní y frutos secos", "Alfajores"),
            new("Golosinas", "Chicles", "Caramelos", "Chocolates"),
            new("Limpieza", "Detergentes", "Lavandina", "Jabón en polvo"),
            new("Cuidado personal", "Shampoo", "Jabón de tocador", "Pañales"),
            new("Congelados", "Helados", "Verduras congeladas"),
            new("Fiambres", "Jamón", "Salame", "Queso untables"),
            new("Panadería", "Pan", "Facturas"),
            new("Bebidas alcohólicas", "Cervezas", "Vinos")
        ]),
        new("refrigeracion", "Local de refrigeración / electro", "Heladeras, aires, lavarropas y electrodomésticos.",
        [
            new("Heladeras", "Heladeras no frost", "Heladeras con freezer", "Freezers"),
            new("Aires acondicionados", "Split", "Portátiles", "Ventana"),
            new("Lavarropas", "Automáticos", "Semiautomáticos"),
            new("Cocinas y hornos", "Cocinas", "Hornos eléctricos", "Anafes"),
            new("Microondas"),
            new("Termotanques y calefones"),
            new("Pequeños electrodomésticos", "Licuadoras", "Pava eléctrica", "Sandwicheras"),
            new("TV y audio", "Televisores", "Audio"),
            new("Accesorios y repuestos")
        ]),
        new("ferreteria", "Ferretería", "Herramientas, pinturería, electricidad y plomería.",
        [
            new("Herramientas", "Manuales", "Eléctricas"),
            new("Pinturería", "Látex", "Esmaltes", "Diluyentes"),
            new("Electricidad", "Cables", "Interruptores", "Lámparas"),
            new("Plomería", "Caños", "Canillas", "Accesorios"),
            new("Seguridad", "Candados", "Alarmas"),
            new("Jardín", "Mangueras", "Herramientas de jardín")
        ]),
        new("farmacia", "Farmacia / perfumería", "Venta libre, cuidado personal y bebé.",
        [
            new("Medicamentos de venta libre"),
            new("Cuidado personal", "Shampoo", "Crema dental", "Desodorantes"),
            new("Bebé", "Pañales", "Leches maternizadas"),
            new("Primeros auxilios"),
            new("Vitaminas y suplementos")
        ]),
        new("indumentaria", "Indumentaria", "Ropa y calzado por género.",
        [
            new("Mujer", "Remeras", "Pantalones", "Calzado"),
            new("Hombre", "Remeras", "Pantalones", "Calzado"),
            new("Niños", "Remeras", "Calzado"),
            new("Accesorios")
        ]),
        new("verduleria", "Verdulería / dietética", "Frutas, verduras y secos.",
        [
            new("Frutas"),
            new("Verduras"),
            new("Huevos"),
            new("Secos y legumbres"),
            new("Hierbas y condimentos")
        ])
    ];

    public static PlantillaCategoria? PorClave(string? clave)
    {
        var limpio = (clave ?? "").Trim().ToLowerInvariant();
        return Todas.FirstOrDefault(p => p.Clave == limpio);
    }

    /// <summary>Crea solo las raíces que faltan. Después de guardar, llamar <see cref="AplicarHijos"/>.</summary>
    public static List<Categoria> AplicarRaices(int idLocal, PlantillaCategoria plantilla, List<Categoria> delLocal)
    {
        var nuevas = new List<Categoria>();
        foreach (var raiz in plantilla.Raices)
        {
            if (delLocal.Any(c => string.Equals(c.Nombre, raiz.Nombre, StringComparison.OrdinalIgnoreCase)))
                continue;
            var creada = ServicioCategorias.Nueva(idLocal, raiz.Nombre, delLocal);
            delLocal.Add(creada);
            nuevas.Add(creada);
        }
        return nuevas;
    }

    /// <summary>Crea subcategorías bajo las raíces ya guardadas. No borra ni renombra.</summary>
    public static List<Categoria> AplicarHijos(int idLocal, PlantillaCategoria plantilla, List<Categoria> delLocal)
    {
        var nuevas = new List<Categoria>();
        foreach (var raiz in plantilla.Raices)
        {
            var padre = delLocal.FirstOrDefault(c =>
                string.Equals(c.Nombre, raiz.Nombre, StringComparison.OrdinalIgnoreCase));
            if (padre == null || padre.Id <= 0) continue;
            foreach (var hijo in raiz.Hijos)
            {
                if (delLocal.Any(c => string.Equals(c.Nombre, hijo.Nombre, StringComparison.OrdinalIgnoreCase)))
                    continue;
                var creada = ServicioCategorias.Nueva(idLocal, hijo.Nombre, delLocal, padre.Id, padre);
                delLocal.Add(creada);
                nuevas.Add(creada);
            }
        }
        return nuevas;
    }
}
