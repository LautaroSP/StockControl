namespace StockControl.Dominio;

public record ResultadoDeshacer(int Restaurados, IReadOnlyList<string> Omitidos);

/// <summary>
/// Guarda cada aumento y deshace solo el último: vuelve lo que nadie tocó después
/// (precio y costo actuales iguales a los que dejó el aumento); lo demás se lista y queda como está.
/// </summary>
public static class ServicioDeshacerAumento
{
    public static AumentoHistorial Registrar(
        int idLocal, string usuario, Aumento precio, Aumento costo, int redondeo,
        ResultadoAumento resultado, DateTimeOffset fecha) => new()
    {
        IdLocal = idLocal,
        Fecha = fecha,
        Usuario = usuario,
        TipoPrecio = precio.Tipo,
        ValorPrecio = precio.Valor,
        TipoCosto = costo.Tipo,
        ValorCosto = costo.Valor,
        RedondeoPrecio = redondeo,
        Items = resultado.Cambios.Select(c => new AumentoHistorialItem
        {
            IdLocal = idLocal,
            IdProducto = c.IdProducto,
            IdGrupoProducto = c.IdGrupoProducto,
            Nombre = c.Nombre,
            PrecioAntes = c.PrecioAntes,
            PrecioDespues = c.PrecioDespues,
            CostoAntes = c.CostoAntes,
            CostoDespues = c.CostoDespues
        }).ToList()
    };

    public static AumentoHistorial? Ultimo(IEnumerable<AumentoHistorial> historiales) =>
        historiales.OrderByDescending(h => h.Fecha).FirstOrDefault();

    public static ResultadoDeshacer Deshacer(
        IEnumerable<AumentoHistorial> historiales,
        IReadOnlyList<Producto> productos,
        IReadOnlyList<GrupoProductos> grupos,
        string usuario)
    {
        var ultimo = Ultimo(historiales) ?? throw new ErrorNegocio("No hay aumentos para deshacer.");
        if (ultimo.Deshecho)
            throw new ErrorNegocio("El último aumento ya se deshizo.");

        var restaurados = 0;
        var omitidos = new List<string>();
        foreach (var item in ultimo.Items)
        {
            if (Restaurar(item, productos, grupos, usuario))
                restaurados++;
            else
                omitidos.Add(item.Nombre);
        }
        ultimo.Deshecho = true;
        ultimo.FechaDeshecho = DateTimeOffset.UtcNow;
        ultimo.UsuarioDeshizo = usuario;
        return new ResultadoDeshacer(restaurados, omitidos);
    }

    static bool Restaurar(AumentoHistorialItem item, IReadOnlyList<Producto> productos, IReadOnlyList<GrupoProductos> grupos, string usuario)
    {
        if (item.IdGrupoProducto is int idGrupo)
        {
            var grupo = grupos.FirstOrDefault(g => g.IdGrupoProducto == idGrupo);
            if (grupo == null || grupo.PrecioGrupo != item.PrecioDespues || grupo.Costo != item.CostoDespues)
                return false;
            grupo.PrecioGrupo = item.PrecioAntes;
            grupo.Costo = item.CostoAntes;
            ServicioGrupo.AplicarAMiembros(grupo, productos.Where(p => p.IdGrupoProducto == idGrupo), usuario);
            return true;
        }
        var producto = productos.FirstOrDefault(p => p.Id == item.IdProducto);
        if (producto == null || producto.Precio != item.PrecioDespues || producto.Costo != item.CostoDespues)
            return false;
        producto.Precio = item.PrecioAntes;
        producto.Costo = item.CostoAntes;
        producto.FechaModificacion = DateTimeOffset.UtcNow;
        producto.UsuarioModificacion = usuario;
        return true;
    }
}
