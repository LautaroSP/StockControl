namespace StockControl.Dominio;

public sealed class FilaLista
{
    public int Fila { get; init; }
    public string Codigo { get; init; } = "";
    /// <summary>Costo del pack (o de la unidad si no tiene pack). Null = la celda no era un número.</summary>
    public decimal? Costo { get; init; }
}

public sealed class FilaListaEncontrada
{
    public int Fila { get; init; }
    public required string Codigo { get; init; }
    public required string Nombre { get; init; }
    public decimal CostoPack { get; init; }
    public int UnidadesPack { get; init; }
    public decimal CostoAntes { get; init; }
    public decimal CostoNuevo { get; init; }
    public decimal PrecioAntes { get; init; }
    public decimal PrecioNuevo { get; init; }
    public string Aviso { get; init; } = "";
}

public sealed class FilaListaNoEncontrada
{
    public int Fila { get; init; }
    public required string Codigo { get; init; }
}

public sealed class FilaListaSalteada
{
    public int Fila { get; init; }
    public string Codigo { get; init; } = "";
    public required string Motivo { get; init; }
}

public sealed class RevisionLista
{
    public List<FilaListaEncontrada> Encontrados { get; } = new();
    public List<FilaListaNoEncontrada> NoEncontrados { get; } = new();
    public List<FilaListaSalteada> Salteados { get; } = new();
}

/// <summary>
/// Lista de costos del proveedor (código de barras + costo): actualiza costo y precio de lo que ya existe.
/// No crea productos ni suma stock (eso es del catálogo, <see cref="ServicioCatalogoProveedor"/>).
/// </summary>
public static class ServicioListaProveedor
{
    public static decimal CostoUnitario(decimal costoPack, int unidadesPack) =>
        Math.Round(costoPack / Math.Max(1, unidadesPack), 2, MidpointRounding.AwayFromZero);

    public static decimal Precio(decimal costo, decimal factorGananciaLocal, decimal factorPropio, bool gananciaIndividual, decimal iva) =>
        PrecioVenta.CalcularLista(costo, gananciaIndividual ? factorPropio : factorGananciaLocal, iva);

    public static RevisionLista Revisar(
        IEnumerable<FilaLista> filas,
        IReadOnlyList<Producto> productos,
        IReadOnlyList<GrupoProductos> grupos,
        IEnumerable<ProductoProveedor> vinculos,
        int idProveedor,
        decimal factorGananciaLocal,
        decimal iva) =>
        Calcular(filas, productos, grupos, vinculos, idProveedor, factorGananciaLocal, iva).Revision;

    public static RevisionLista Aplicar(
        IEnumerable<FilaLista> filas,
        IReadOnlyList<Producto> productos,
        IReadOnlyList<GrupoProductos> grupos,
        IEnumerable<ProductoProveedor> vinculos,
        int idProveedor,
        decimal factorGananciaLocal,
        decimal iva,
        string usuario)
    {
        var plan = Calcular(filas, productos, grupos, vinculos, idProveedor, factorGananciaLocal, iva);
        foreach (var c in plan.Cambios)
            Escribir(c, productos, usuario);
        return plan.Revision;
    }

    private sealed record Cambio(Producto Producto, GrupoProductos? Grupo, decimal Costo, decimal Precio);

    private sealed record Plan(RevisionLista Revision, List<Cambio> Cambios);

    private static void Escribir(Cambio c, IReadOnlyList<Producto> productos, string usuario)
    {
        if (c.Grupo == null)
        {
            c.Producto.Costo = c.Costo;
            c.Producto.Precio = c.Precio;
            c.Producto.FechaModificacion = DateTimeOffset.UtcNow;
            c.Producto.UsuarioModificacion = usuario;
            return;
        }
        c.Grupo.Costo = c.Costo;
        c.Grupo.PrecioGrupo = c.Precio;
        var miembros = productos.Where(p => p.IdGrupoProducto == c.Grupo.IdGrupoProducto);
        ServicioGrupo.AplicarAMiembros(c.Grupo, miembros, usuario);
    }

    private static Plan Calcular(
        IEnumerable<FilaLista> filas,
        IReadOnlyList<Producto> productos,
        IReadOnlyList<GrupoProductos> grupos,
        IEnumerable<ProductoProveedor> vinculos,
        int idProveedor,
        decimal factorLocal,
        decimal iva)
    {
        var porCodigo = productos.GroupBy(p => p.Codigo, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var packs = vinculos.Where(v => v.IdProveedor == idProveedor)
            .GroupBy(v => v.IdProducto).ToDictionary(g => g.Key, g => g.First().UnidadesPack);
        var gruposPorId = grupos.ToDictionary(g => g.IdGrupoProducto);
        var vistos = new HashSet<string>(StringComparer.Ordinal);
        var gruposVistos = new HashSet<int>();
        var plan = new Plan(new RevisionLista(), new List<Cambio>());

        foreach (var fila in filas)
        {
            var codigo = fila.Codigo.Trim();
            porCodigo.TryGetValue(codigo, out var p);
            var motivo = Motivo(fila, codigo, p, idProveedor, vistos);
            vistos.Add(codigo);
            if (motivo == null && p == null)
            {
                plan.Revision.NoEncontrados.Add(new FilaListaNoEncontrada { Fila = fila.Fila, Codigo = codigo });
                continue;
            }
            if (motivo == null && p!.IdGrupoProducto > 0 && !gruposVistos.Add(p.IdGrupoProducto))
                motivo = "Otro producto del mismo grupo ya actualizó el grupo.";
            if (motivo != null)
            {
                plan.Revision.Salteados.Add(new FilaListaSalteada { Fila = fila.Fila, Codigo = codigo, Motivo = motivo });
                continue;
            }
            Agregar(plan, fila, codigo, p!, packs.GetValueOrDefault(p!.Id, 1), gruposPorId, factorLocal, iva);
        }
        return plan;
    }

    private static string? Motivo(FilaLista fila, string codigo, Producto? p, int idProveedor, HashSet<string> vistos)
    {
        if (codigo.Length == 0) return "Sin código.";
        if (codigo.StartsWith("GENERIC-", StringComparison.OrdinalIgnoreCase)) return "Los genéricos no se actualizan.";
        if (vistos.Contains(codigo)) return "Código repetido en el Excel.";
        if (fila.Costo is not > 0) return "El costo tiene que ser mayor a 0.";
        if (p == null) return null;
        if (p.ProductoSector) return "Es un producto sector: no tiene precio de lista.";
        if (p.IdProveedor != idProveedor) return "No es de este proveedor.";
        return null;
    }

    private static void Agregar(
        Plan plan, FilaLista fila, string codigo, Producto p, int unidadesPack,
        Dictionary<int, GrupoProductos> gruposPorId, decimal factorLocal, decimal iva)
    {
        var costo = CostoUnitario(fila.Costo!.Value, unidadesPack);
        GrupoProductos? grupo = null;
        if (p.IdGrupoProducto > 0 && !gruposPorId.TryGetValue(p.IdGrupoProducto, out grupo))
        {
            plan.Revision.Salteados.Add(new FilaListaSalteada { Fila = fila.Fila, Codigo = codigo, Motivo = "El grupo no existe." });
            return;
        }

        var precio = grupo != null
            ? Precio(costo, factorLocal, grupo.Ganancia, grupo.GananciaIndividual, iva)
            : Precio(costo, factorLocal, p.ValorGanancia, p.GananciaIndividual, iva);
        plan.Cambios.Add(new Cambio(p, grupo, costo, precio));
        plan.Revision.Encontrados.Add(new FilaListaEncontrada
        {
            Fila = fila.Fila,
            Codigo = codigo,
            Nombre = p.Nombre,
            CostoPack = fila.Costo.Value,
            UnidadesPack = Math.Max(1, unidadesPack),
            CostoAntes = grupo?.Costo ?? p.Costo,
            CostoNuevo = costo,
            PrecioAntes = grupo?.PrecioGrupo ?? p.Precio,
            PrecioNuevo = precio,
            Aviso = grupo == null ? "" : $"Está en el grupo {grupo.NombreGrupo}: se actualiza el grupo."
        });
    }
}
