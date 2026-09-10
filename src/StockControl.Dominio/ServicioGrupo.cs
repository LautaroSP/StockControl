namespace StockControl.Dominio;

public static class ServicioGrupo
{
    public static void AplicarAMiembros(GrupoProductos grupo, IEnumerable<Producto> miembros, string usuarioModificacion = "")
    {
        foreach (var p in miembros)
        {
            if (p.ProductoSector)
                continue;
            p.IdGrupoProducto = grupo.IdGrupoProducto;
            p.Costo = grupo.Costo;
            p.Precio = grupo.PrecioGrupo;
            p.FechaModificacion = DateTimeOffset.UtcNow;
            p.UsuarioModificacion = usuarioModificacion;
        }
    }

    public static void Asignar(Producto producto, GrupoProductos grupo, string usuarioModificacion = "")
    {
        if (producto.ProductoSector)
            throw new ErrorNegocio("Un producto sector no entra a un grupo.");
        producto.IdGrupoProducto = grupo.IdGrupoProducto;
        producto.Costo = grupo.Costo;
        producto.Precio = grupo.PrecioGrupo;
        producto.FechaModificacion = DateTimeOffset.UtcNow;
        producto.UsuarioModificacion = usuarioModificacion;
    }

    public static void Sacar(Producto producto, string usuarioModificacion = "")
    {
        producto.IdGrupoProducto = 0;
        producto.FechaModificacion = DateTimeOffset.UtcNow;
        producto.UsuarioModificacion = usuarioModificacion;
    }

    public static void Eliminar(IEnumerable<Producto> miembros, string usuarioModificacion = "")
    {
        foreach (var p in miembros)
            Sacar(p, usuarioModificacion);
    }

    /// <summary>
    /// Deja el grupo con exactamente esos ids. Los que salen quedan en 0; los que entran reciben precio/costo del grupo.
    /// </summary>
    public static void ReemplazarMiembros(
        GrupoProductos grupo,
        IReadOnlyList<Producto> candidatos,
        IReadOnlyCollection<int> idsNuevos,
        string usuarioModificacion = "")
    {
        var set = idsNuevos.ToHashSet();
        foreach (var p in candidatos)
        {
            var debeEstar = set.Contains(p.Id);
            var esta = p.IdGrupoProducto == grupo.IdGrupoProducto;
            if (debeEstar && !esta)
                Asignar(p, grupo, usuarioModificacion);
            else if (!debeEstar && esta)
                Sacar(p, usuarioModificacion);
            else if (debeEstar && esta)
            {
                p.Costo = grupo.Costo;
                p.Precio = grupo.PrecioGrupo;
                p.FechaModificacion = DateTimeOffset.UtcNow;
                p.UsuarioModificacion = usuarioModificacion;
            }
        }
    }
}
