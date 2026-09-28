namespace StockControl.Dominio;

public sealed class FilaCatalogo
{
    public int Fila { get; init; }
    public string Codigo { get; init; } = "";
    public string Nombre { get; init; } = "";
    /// <summary>Null = la celda no era un número.</summary>
    public decimal? Costo { get; init; }
    /// <summary>Null = la celda no era un número. Vacía = 0.</summary>
    public decimal? Cantidad { get; init; }
}

public sealed class FilaRevisada
{
    public int Fila { get; init; }
    public required string Codigo { get; init; }
    public required string Nombre { get; init; }
    public decimal Costo { get; init; }
    public decimal Cantidad { get; init; }
    public required string Accion { get; init; }
    public decimal Precio { get; init; }
    public string Aviso { get; init; } = "";
}

public sealed class FilaInvalida
{
    public int Fila { get; init; }
    public string Codigo { get; init; } = "";
    public required string Motivo { get; init; }
}

public sealed class RevisionCatalogo
{
    public List<FilaRevisada> Validas { get; } = new();
    public List<FilaInvalida> Invalidas { get; } = new();
}

public sealed class ResultadoCatalogo
{
    public List<Producto> Nuevos { get; } = new();
    public int Actualizados { get; set; }
    public List<FilaInvalida> Invalidas { get; init; } = new();
}

public static class ServicioCatalogoProveedor
{
    public const string Crear = "crear";
    public const string Actualizar = "actualizar";

    public static RevisionCatalogo Revisar(
        IEnumerable<FilaCatalogo> filas,
        IEnumerable<Producto> productosDelLocal,
        int idProveedor,
        decimal factorGanancia,
        decimal iva)
    {
        var porCodigo = productosDelLocal.ToDictionary(p => p.Codigo, StringComparer.Ordinal);
        var vistos = new HashSet<string>(StringComparer.Ordinal);
        var revision = new RevisionCatalogo();

        foreach (var fila in filas)
        {
            var codigo = fila.Codigo.Trim();
            porCodigo.TryGetValue(codigo, out var existente);
            var motivo = MotivoInvalida(fila, codigo, existente, vistos);
            if (motivo != null)
            {
                revision.Invalidas.Add(new FilaInvalida { Fila = fila.Fila, Codigo = codigo, Motivo = motivo });
                continue;
            }
            vistos.Add(codigo);
            revision.Validas.Add(Revisada(fila, codigo, existente, idProveedor, factorGanancia, iva));
        }
        return revision;
    }

    public static ResultadoCatalogo Aplicar(
        IEnumerable<FilaCatalogo> filas,
        IReadOnlyList<Producto> productosDelLocal,
        int idProveedor,
        int idLocal,
        decimal factorGanancia,
        decimal iva,
        string usuario)
    {
        var revision = Revisar(filas, productosDelLocal, idProveedor, factorGanancia, iva);
        var porCodigo = productosDelLocal.ToDictionary(p => p.Codigo, StringComparer.Ordinal);
        var resultado = new ResultadoCatalogo { Invalidas = revision.Invalidas };
        var ahora = DateTimeOffset.UtcNow;

        foreach (var f in revision.Validas)
        {
            if (porCodigo.TryGetValue(f.Codigo, out var p))
            {
                p.Costo = f.Costo;
                p.Precio = f.Precio;
                p.Cantidad += f.Cantidad;
                p.IdProveedor = idProveedor;
                p.FechaModificacion = ahora;
                p.UsuarioModificacion = usuario;
                resultado.Actualizados++;
                continue;
            }
            resultado.Nuevos.Add(new Producto
            {
                IdLocal = idLocal,
                Codigo = f.Codigo,
                Nombre = f.Nombre,
                Costo = f.Costo,
                Precio = f.Precio,
                Cantidad = f.Cantidad,
                IdProveedor = idProveedor,
                FechaModificacion = ahora,
                UsuarioModificacion = usuario
            });
        }
        return resultado;
    }

    private static string? MotivoInvalida(FilaCatalogo fila, string codigo, Producto? existente, HashSet<string> vistos)
    {
        if (codigo.Length == 0) return "Sin código.";
        if (codigo.Length > 80 || fila.Nombre.Length > 200) return "Código o nombre demasiado largo.";
        if (codigo.StartsWith("GENERIC-", StringComparison.OrdinalIgnoreCase)) return "Los genéricos no se importan.";
        if (vistos.Contains(codigo)) return "Código repetido en el Excel.";
        if (string.IsNullOrWhiteSpace(fila.Nombre)) return "Sin nombre de producto.";
        if (fila.Costo is not > 0) return "El costo tiene que ser mayor a 0.";
        if (fila.Cantidad is null or < 0) return "Cantidad inválida.";
        if (existente?.ProductoSector == true) return "Es un producto sector: no se importa.";
        return null;
    }

    private static FilaRevisada Revisada(
        FilaCatalogo fila,
        string codigo,
        Producto? existente,
        int idProveedor,
        decimal factorGanancia,
        decimal iva)
    {
        var costo = fila.Costo!.Value;
        var cantidad = fila.Cantidad!.Value;
        if (existente == null)
        {
            return new FilaRevisada
            {
                Fila = fila.Fila,
                Codigo = codigo,
                Nombre = fila.Nombre.Trim(),
                Costo = costo,
                Cantidad = cantidad,
                Accion = Crear,
                Precio = PrecioVenta.CalcularLista(costo, factorGanancia, iva)
            };
        }

        var avisos = new List<string> { $"Ya existe: suma {cantidad:0.###} al stock." };
        decimal precio;
        if (existente.IdGrupoProducto > 0)
        {
            precio = existente.Precio;
            costo = existente.Costo;
            avisos.Add("Está en un grupo: costo y precio los pone el grupo.");
        }
        else if (existente.GananciaIndividual)
        {
            precio = existente.Precio;
            avisos.Add("Tiene ganancia propia: se mantiene el precio.");
        }
        else
        {
            precio = PrecioVenta.CalcularLista(costo, factorGanancia, iva);
        }
        if (existente.IdProveedor > 0 && existente.IdProveedor != idProveedor)
            avisos.Add("Pasa a este proveedor.");

        return new FilaRevisada
        {
            Fila = fila.Fila,
            Codigo = codigo,
            Nombre = existente.Nombre,
            Costo = costo,
            Cantidad = cantidad,
            Accion = Actualizar,
            Precio = precio,
            Aviso = string.Join(" ", avisos)
        };
    }
}
