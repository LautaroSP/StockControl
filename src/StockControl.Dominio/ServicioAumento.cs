namespace StockControl.Dominio;

public static class TipoAumento
{
    public const string Ninguno = "ninguno";
    public const string Porcentaje = "porcentaje";
    public const string Importe = "importe";
}

public readonly record struct Aumento(string Tipo, decimal Valor)
{
    public static Aumento Nada { get; } = new(TipoAumento.Ninguno, 0);

    public bool EsNada => Tipo == TipoAumento.Ninguno || Valor == 0;
}

/// <summary>
/// Sube precio y costo por separado, en porcentaje o en pesos.
/// El producto de un grupo mueve el grupo una sola vez y, con él, a todos sus miembros.
/// El sector no tiene precio de lista: solo se le sube el costo.
/// </summary>
public static class ServicioAumento
{
    public static Aumento Normalizar(string? tipo, decimal valor)
    {
        var t = (tipo ?? "").Trim();
        if (t is "" or TipoAumento.Ninguno || valor == 0)
            return Aumento.Nada;
        if (t is not (TipoAumento.Porcentaje or TipoAumento.Importe))
            throw new ErrorNegocio("El aumento es un porcentaje o un importe.");
        if (valor <= 0)
            throw new ErrorNegocio("El aumento tiene que ser mayor a 0.");
        if (t == TipoAumento.Porcentaje && valor > 1000)
            throw new ErrorNegocio("El porcentaje de aumento va hasta 1000.");
        return new Aumento(t, valor);
    }

    public static void ExigirAlguno(Aumento precio, Aumento costo)
    {
        if (precio.EsNada && costo.EsNada)
            throw new ErrorNegocio("Indicá un aumento de precio o de costo.");
    }

    public static List<int> IdsDeCategoria(int idCategoria, IEnumerable<Producto> productos, IEnumerable<GrupoProductos> grupos)
    {
        if (idCategoria <= 0)
            throw new ErrorNegocio("Elegí una categoría.");
        var porId = grupos.ToDictionary(g => g.IdGrupoProducto);
        return productos
            .Where(p => !p.EsGenerico && ServicioCategorias.Efectiva(p, porId.GetValueOrDefault(p.IdGrupoProducto)) == idCategoria)
            .Select(p => p.Id)
            .ToList();
    }

    public static List<int> IdsDeProveedor(int idProveedor, IEnumerable<Producto> productos)
    {
        if (idProveedor <= 0)
            throw new ErrorNegocio("Elegí un proveedor.");
        return productos
            .Where(p => !p.EsGenerico && p.IdProveedor == idProveedor)
            .Select(p => p.Id)
            .ToList();
    }

    public static int NormalizarRedondeo(int multiplo)
    {
        if (multiplo is not (0 or 10 or 50 or 100))
            throw new ErrorNegocio("El redondeo es a 10, 50 o 100.");
        return multiplo;
    }

    public static decimal Redondear(decimal precio, int multiplo)
    {
        if (multiplo == 0)
            return precio;
        return Math.Ceiling(precio / multiplo) * multiplo;
    }

    public static ResultadoAumento Aplicar(
        IReadOnlyList<Producto> productos,
        IReadOnlyList<GrupoProductos> grupos,
        IReadOnlyCollection<int> ids,
        Aumento precio,
        Aumento costo,
        string usuario,
        int redondeo = 0)
    {
        precio = Normalizar(precio.Tipo, precio.Valor);
        costo = Normalizar(costo.Tipo, costo.Valor);
        redondeo = NormalizarRedondeo(redondeo);
        ExigirAlguno(precio, costo);
        var set = ids.ToHashSet();
        var elegidos = productos.Where(p => set.Contains(p.Id) && !p.EsGenerico).ToList();
        if (elegidos.Count == 0)
            throw new ErrorNegocio("Elegí al menos un producto.");

        var gruposPorId = grupos.ToDictionary(g => g.IdGrupoProducto);
        var gruposVistos = new HashSet<int>();
        var cambios = new List<CambioAumento>();
        var cantidad = 0;
        foreach (var p in elegidos)
        {
            if (p.IdGrupoProducto > 0)
            {
                if (!gruposVistos.Add(p.IdGrupoProducto))
                    continue;
                if (!gruposPorId.TryGetValue(p.IdGrupoProducto, out var grupo))
                    throw new ErrorNegocio("El grupo no existe.");
                cantidad += AumentarGrupo(grupo, productos, precio, costo, redondeo, usuario, cambios);
                continue;
            }
            if (AumentarProducto(p, precio, costo, redondeo, usuario, cambios))
                cantidad++;
        }

        if (cantidad == 0)
            throw new ErrorNegocio("Esos productos no tienen precio de lista.");
        return new ResultadoAumento(cantidad, cambios);
    }

    /// <summary>Misma cuenta que <see cref="Aplicar"/> sobre copias: no toca las entidades.</summary>
    public static ResultadoAumento Simular(
        IReadOnlyList<Producto> productos,
        IReadOnlyList<GrupoProductos> grupos,
        IReadOnlyCollection<int> ids,
        Aumento precio,
        Aumento costo,
        int redondeo = 0)
    {
        var copiaProductos = productos.Select(Copiar).ToList();
        var copiaGrupos = grupos.Select(Copiar).ToList();
        return Aplicar(copiaProductos, copiaGrupos, ids, precio, costo, "", redondeo);
    }

    static int AumentarGrupo(
        GrupoProductos grupo, IReadOnlyList<Producto> productos, Aumento precio, Aumento costo,
        int redondeo, string usuario, List<CambioAumento> cambios)
    {
        var precioAntes = grupo.PrecioGrupo;
        var costoAntes = grupo.Costo;
        if (!precio.EsNada)
            grupo.PrecioGrupo = Redondear(Nuevo(grupo.PrecioGrupo, precio), redondeo);
        if (!costo.EsNada)
            grupo.Costo = Nuevo(grupo.Costo, costo);
        var miembros = productos.Where(x => x.IdGrupoProducto == grupo.IdGrupoProducto).ToList();
        ServicioGrupo.AplicarAMiembros(grupo, miembros, usuario);
        cambios.Add(new CambioAumento(null, grupo.IdGrupoProducto, grupo.NombreGrupo,
            precioAntes, grupo.PrecioGrupo, costoAntes, grupo.Costo));
        return miembros.Count;
    }

    static bool AumentarProducto(
        Producto p, Aumento precio, Aumento costo, int redondeo, string usuario, List<CambioAumento> cambios)
    {
        var precioAntes = p.Precio;
        var costoAntes = p.Costo;
        var cambio = false;
        if (!costo.EsNada)
        {
            p.Costo = Nuevo(p.Costo, costo);
            cambio = true;
        }
        if (!p.ProductoSector && !precio.EsNada)
        {
            p.Precio = Redondear(Nuevo(p.Precio, precio), redondeo);
            cambio = true;
        }
        if (!cambio)
            return false;
        p.FechaModificacion = DateTimeOffset.UtcNow;
        p.UsuarioModificacion = usuario;
        cambios.Add(new CambioAumento(p.Id, null, p.Nombre, precioAntes, p.Precio, costoAntes, p.Costo));
        return true;
    }

    static T Copiar<T>(T entidad) where T : class =>
        (T)MetodoClonar.Invoke(entidad, null)!;

    static readonly System.Reflection.MethodInfo MetodoClonar =
        typeof(object).GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

    static decimal Nuevo(decimal actual, Aumento ajuste)
    {
        var nuevo = ajuste.Tipo == TipoAumento.Porcentaje
            ? actual * (1 + ajuste.Valor / 100m)
            : actual + ajuste.Valor;
        return Math.Round(nuevo, 2, MidpointRounding.AwayFromZero);
    }
}

public readonly record struct CambioAumento(
    int? IdProducto, int? IdGrupoProducto, string Nombre,
    decimal PrecioAntes, decimal PrecioDespues, decimal CostoAntes, decimal CostoDespues);

public record ResultadoAumento(int Cantidad, IReadOnlyList<CambioAumento> Cambios);
