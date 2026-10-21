using System.Security.Cryptography;
using System.Text;

namespace StockControl.Dominio;

public sealed class CopiaCatalogo
{
    public List<GrupoProductos> Grupos { get; } = [];
    public Dictionary<int, GrupoProductos> GrupoPorIdOrigen { get; } = [];
    public List<Proveedor> Proveedores { get; } = [];
    public Dictionary<int, Proveedor> ProveedorPorIdOrigen { get; } = [];
    public List<ProductoCopiado> Productos { get; } = [];
}

public readonly record struct ProductoCopiado(Producto Producto, int IdGrupoOrigen, int IdProveedorOrigen);

public static class ServicioAdmin
{
    public static bool PuedeEntrar(string rol) => rol == Roles.Admin;

    public static bool EsCuentaRegistrada(string rol) =>
        rol is Roles.Dueno or Roles.Admin;

    public static bool EsUsuarioDelDueno(string rol) =>
        rol is Roles.Empleado or Roles.Socio;

    public static void ValidarClaveSecreta(string? ingresada, string? configurada)
    {
        if (string.IsNullOrEmpty(configurada))
            throw new ErrorNegocio("No hay clave secreta configurada.");
        var pedida = SHA256.HashData(Encoding.UTF8.GetBytes(ingresada ?? ""));
        var guardada = SHA256.HashData(Encoding.UTF8.GetBytes(configurada));
        if (!CryptographicOperations.FixedTimeEquals(pedida, guardada))
            throw new ErrorNegocio("La clave secreta no coincide.");
    }

    public static Local NuevoLocal(string nombre, DateTimeOffset ahora)
    {
        var limpio = NombreLocal(nombre);
        return new Local
        {
            Nombre = limpio,
            EstadoAbono = ServicioAbono.Trial,
            Vence = ServicioAbono.VenceTrial(ahora)
        };
    }

    /// <summary>Sucursal que el dueño contrata: nace esperando el pago, sin los 7 días de prueba.</summary>
    public static Local ContratarLocal(string nombre) =>
        new() { Nombre = NombreLocal(nombre), EstadoAbono = ServicioAbono.Pendiente, Vence = null };

    /// <summary>El trial es uno por dueño. Si ya tiene un local, el siguiente espera el pago.</summary>
    public static Local AltaLocal(string nombre, DateTimeOffset ahora, bool yaTieneLocal) =>
        yaTieneLocal ? ContratarLocal(nombre) : NuevoLocal(nombre, ahora);

    /// <summary>Copia el catálogo a otro local. El stock se repite o queda en cero. No copia genéricos.</summary>
    public static CopiaCatalogo CopiarCatalogo(
        IEnumerable<GrupoProductos> grupos,
        IEnumerable<Proveedor> proveedores,
        IEnumerable<Producto> productos,
        int idLocalDestino,
        bool copiarStock,
        DateTimeOffset ahora,
        string usuario)
    {
        var copia = new CopiaCatalogo();
        foreach (var grupo in grupos)
        {
            var nuevo = new GrupoProductos
            {
                IdLocal = idLocalDestino,
                NombreGrupo = grupo.NombreGrupo,
                PrecioGrupo = grupo.PrecioGrupo,
                Costo = grupo.Costo,
                Ganancia = grupo.Ganancia,
                GananciaIndividual = grupo.GananciaIndividual
            };
            copia.Grupos.Add(nuevo);
            copia.GrupoPorIdOrigen[grupo.IdGrupoProducto] = nuevo;
        }

        foreach (var proveedor in proveedores)
        {
            var nuevo = new Proveedor
            {
                IdLocal = idLocalDestino,
                Nombre = proveedor.Nombre,
                TipoProductos = proveedor.TipoProductos,
                Telefono = proveedor.Telefono,
                Activo = proveedor.Activo
            };
            copia.Proveedores.Add(nuevo);
            copia.ProveedorPorIdOrigen[proveedor.Id] = nuevo;
        }

        foreach (var producto in productos)
        {
            if (producto.EsGenerico) continue;
            copia.Productos.Add(new ProductoCopiado(
                new Producto
                {
                    IdLocal = idLocalDestino,
                    Codigo = producto.Codigo,
                    Nombre = producto.Nombre,
                    Cantidad = copiarStock ? producto.Cantidad : 0,
                    Costo = producto.Costo,
                    Precio = producto.Precio,
                    ProductoSector = producto.ProductoSector,
                    GananciaIndividual = producto.GananciaIndividual,
                    ValorGanancia = producto.ValorGanancia,
                    FechaModificacion = ahora,
                    UsuarioModificacion = usuario
                },
                producto.IdGrupoProducto,
                producto.IdProveedor));
        }

        return copia;
    }

    static string NombreLocal(string nombre)
    {
        var limpio = nombre.Trim();
        if (limpio.Length == 0 || limpio.Length > 200)
            throw new ErrorNegocio("El nombre del local es obligatorio.");
        return limpio;
    }

    public static void ValidarDueno(string rol)
    {
        if (!EsCuentaRegistrada(rol))
            throw new ErrorNegocio("El local se asocia a un dueño.");
    }

    public static void Bloquear(Usuario usuario, int idActor)
    {
        if (usuario.Id == idActor)
            throw new ErrorNegocio("No podés bloquear tu propio usuario.");
        usuario.Activo = false;
    }

    public static void ValidarEliminar(int idActor, int idObjetivo)
    {
        if (idActor == idObjetivo)
            throw new ErrorNegocio("No podés eliminar tu propio usuario.");
    }

    public static void AsegurarAdminRestante(string rol, int otrosAdminsActivos)
    {
        if (rol == Roles.Admin && otrosAdminsActivos == 0)
            throw new ErrorNegocio("Tiene que quedar al menos un administrador.");
    }

    public const string CanalTransferencia = "transferencia";
    public const string CanalEfectivo = "efectivo";
    public const string CanalVentas = "ventas";

    public static string ValidarCanal(string? canal)
    {
        var limpio = (canal ?? "").Trim().ToLowerInvariant();
        if (limpio is not (CanalTransferencia or CanalEfectivo or CanalVentas))
            throw new ErrorNegocio("Elegí transferencia, efectivo o hablé con ventas.");
        return limpio;
    }

    public static void CambiarVencimiento(Local local, DateTimeOffset vence) => local.Vence = vence;

    public static DateTimeOffset InterpretarVencimiento(string texto)
    {
        if (!DateTime.TryParse(texto, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var local)
            && !DateTime.TryParse(texto, new System.Globalization.CultureInfo("es-AR"), System.Globalization.DateTimeStyles.None, out local))
            throw new ErrorNegocio("La fecha de vencimiento no es válida.");
        var sinZona = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        return new DateTimeOffset(sinZona, DiaArgentina.Zona.GetUtcOffset(sinZona));
    }

    public static void Desactivar(Local local)
    {
        if (!local.Activo)
            throw new ErrorNegocio("El local ya está desactivado.");
        local.Activo = false;
    }

    public static void Activar(Local local)
    {
        if (local.Activo)
            throw new ErrorNegocio("El local ya está activo.");
        local.Activo = true;
    }

    public static int Empleados(IEnumerable<(int IdLocal, string Rol)> vinculos, int idLocal) =>
        vinculos.Count(v => v.IdLocal == idLocal && EsUsuarioDelDueno(v.Rol));

    public static Usuario NuevoDueno(string nombre, string email, string usuario, string clave)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(usuario))
            throw new ErrorNegocio("Completá nombre, mail y usuario del dueño.");
        var nombreLimpio = nombre.Trim();
        var mail = email.Trim().ToLowerInvariant();
        var usuarioLimpio = usuario.Trim();
        if (nombreLimpio.Length > 120 || mail.Length > 200 || usuarioLimpio.Length > 80)
            throw new ErrorNegocio("Algún dato del dueño es demasiado largo.");
        if (!ServicioRegistro.EsMail(mail))
            throw new ErrorNegocio("El mail no es válido.");
        if (string.IsNullOrEmpty(clave) || clave.Length < ServicioRegistro.LargoMinimoClave || clave.Length > 200)
            throw new ErrorNegocio($"La contraseña debe tener al menos {ServicioRegistro.LargoMinimoClave} caracteres.");
        return new Usuario
        {
            Nombre = nombreLimpio,
            Email = mail,
            NombreUsuario = usuarioLimpio,
            Rol = Roles.Dueno,
            Activo = true
        };
    }

    public const string EmailVentasGenerico = "contacto@stockcontrol.com";

    public static string GuardarTelefonoVentas(string telefono) =>
        ServicioOrdenCompra.NormalizarTelefono(telefono);

    public static string GuardarEmailVentas(string email)
    {
        var mail = email.Trim().ToLowerInvariant();
        if (mail.Length > 200 || !ServicioRegistro.EsMail(mail))
            throw new ErrorNegocio("El mail no es válido.");
        return mail;
    }

    public static string MostrarTelefono(string normalizado)
    {
        if (normalizado.Length == 13 && normalizado.StartsWith("549"))
        {
            var resto = normalizado[5..];
            return $"+54 9 {normalizado[3..5]} {resto[..4]}-{resto[4..]}";
        }
        return string.IsNullOrEmpty(normalizado) ? "" : "+" + normalizado;
    }

    public static string ValidarCambioClave(string rol, string? clave, string? claveSecreta, string? claveConfigurada)
    {
        ValidarClaveSecreta(claveSecreta, claveConfigurada);
        if (rol is not (Roles.Admin or Roles.Dueno or Roles.Empleado or Roles.Socio))
            throw new ErrorNegocio("No se puede cambiar la clave de ese usuario.");
        var limpia = clave?.Trim() ?? "";
        if (limpia.Length < 4)
            throw new ErrorNegocio("La clave debe tener al menos 4 caracteres.");
        if (limpia.Length > 200)
            throw new ErrorNegocio("La clave es demasiado larga.");
        return limpia;
    }
}
