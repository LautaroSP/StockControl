namespace StockControl.Dominio;

public static class ServicioCategorias
{
    public const int LargoMaximoNombre = 40;

    public static Categoria Nueva(int idLocal, string? nombre, IEnumerable<Categoria> delLocal, int? idPadre = null, Categoria? padre = null)
    {
        ValidarPadre(idLocal, idPadre, padre, delLocal);
        return new()
        {
            IdLocal = idLocal,
            Nombre = NombreValido(nombre, delLocal, null),
            IdPadre = idPadre is > 0 ? idPadre : null
        };
    }

    public static void Renombrar(Categoria categoria, string? nombre, IEnumerable<Categoria> delLocal) =>
        categoria.Nombre = NombreValido(nombre, delLocal, categoria.Id);

    /// <summary>Productos con categoría propia y grupos de esa categoría quedan sin categoría. No se borra nada más.</summary>
    public static void Eliminar(Categoria categoria, IEnumerable<Producto> productos, IEnumerable<GrupoProductos> grupos, string usuarioModificacion = "")
    {
        foreach (var producto in productos.Where(p => p.IdCategoria == categoria.Id))
            Asignar(producto, null, usuarioModificacion);
        foreach (var grupo in grupos.Where(g => g.IdCategoria == categoria.Id))
            grupo.IdCategoria = null;
    }

    public static void Asignar(Producto producto, Categoria? categoria, string usuarioModificacion = "")
    {
        ValidarLocal(categoria, producto.IdLocal);
        producto.IdCategoria = categoria?.Id;
        producto.FechaModificacion = DateTimeOffset.UtcNow;
        producto.UsuarioModificacion = usuarioModificacion;
    }

    public static void AsignarAVarios(IEnumerable<Producto> productos, Categoria? categoria, string usuarioModificacion = "")
    {
        foreach (var producto in productos)
            Asignar(producto, categoria, usuarioModificacion);
    }

    public static void AsignarAGrupo(GrupoProductos grupo, Categoria? categoria)
    {
        ValidarLocal(categoria, grupo.IdLocal);
        grupo.IdCategoria = categoria?.Id;
    }

    /// <summary>La propia domina; si no tiene, la del grupo; si tampoco, ninguna.</summary>
    public static int? Efectiva(Producto producto, GrupoProductos? grupo) =>
        producto.IdCategoria
        ?? (grupo != null && producto.IdGrupoProducto != 0 && grupo.IdGrupoProducto == producto.IdGrupoProducto ? grupo.IdCategoria : null);

    /// <summary>Cuántos productos tiene cada categoría, contando los que la heredan del grupo.</summary>
    public static Dictionary<int, int> ContarProductos(IEnumerable<Producto> productos, IEnumerable<GrupoProductos> grupos)
    {
        var porId = grupos.ToDictionary(g => g.IdGrupoProducto);
        return productos
            .Select(p => Efectiva(p, porId.GetValueOrDefault(p.IdGrupoProducto)))
            .OfType<int>()
            .GroupBy(id => id)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>Al filtrar por un padre, incluye también sus hijos.</summary>
    public static List<int> ExpandirConHijos(IReadOnlyCollection<int> ids, IEnumerable<Categoria> todas)
    {
        if (ids.Count == 0) return [];
        var set = ids.ToHashSet();
        foreach (var hija in todas.Where(c => c.IdPadre is int p && set.Contains(p)))
            set.Add(hija.Id);
        return set.ToList();
    }

    /// <summary>Suma a cada padre la cantidad de sus hijos (para el menú lateral).</summary>
    public static Dictionary<int, int> ContarConPadres(Dictionary<int, int> propios, IEnumerable<Categoria> categorias)
    {
        var total = new Dictionary<int, int>(propios);
        foreach (var hija in categorias.Where(c => c.IdPadre is > 0))
        {
            var n = propios.GetValueOrDefault(hija.Id);
            if (n == 0) continue;
            total[hija.IdPadre!.Value] = total.GetValueOrDefault(hija.IdPadre.Value) + n;
        }
        return total;
    }

    static void ValidarPadre(int idLocal, int? idPadre, Categoria? padre, IEnumerable<Categoria> delLocal)
    {
        if (idPadre is not > 0) return;
        padre ??= delLocal.FirstOrDefault(c => c.Id == idPadre);
        if (padre == null || padre.IdLocal != idLocal)
            throw new ErrorNegocio("La categoría padre no es de este local.");
        if (padre.IdPadre != null)
            throw new ErrorNegocio("Solo un nivel de subcategoría.");
    }

    static string NombreValido(string? nombre, IEnumerable<Categoria> delLocal, int? idPropia)
    {
        var limpio = (nombre ?? "").Trim();
        if (limpio.Length == 0)
            throw new ErrorNegocio("El nombre de la categoría es obligatorio.");
        if (limpio.Length > LargoMaximoNombre)
            throw new ErrorNegocio($"El nombre de la categoría tiene hasta {LargoMaximoNombre} caracteres.");
        if (delLocal.Any(c => c.Id != idPropia && string.Equals(c.Nombre.Trim(), limpio, StringComparison.OrdinalIgnoreCase)))
            throw new ErrorNegocio($"Ya existe la categoría «{limpio}».");
        return limpio;
    }

    static void ValidarLocal(Categoria? categoria, int idLocal)
    {
        if (categoria != null && categoria.IdLocal != idLocal)
            throw new ErrorNegocio("La categoría no es de este local.");
    }
}
