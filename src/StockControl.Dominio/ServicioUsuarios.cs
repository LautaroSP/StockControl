namespace StockControl.Dominio;

public static class ServicioUsuarios
{
    /// <summary>
    /// Locales de la cuenta del dueño. El local de la sesión no recorta la lista.
    /// Un socio ve los locales del dueño con el que comparte sucursal, no los de otra cuenta.
    /// </summary>
    public static IReadOnlyList<int> LocalesDeLaCuenta(
        int idUsuario,
        string rol,
        IEnumerable<(int IdUsuario, int IdLocal)> vinculos,
        IEnumerable<(int Id, string Rol)> usuarios)
    {
        var lista = vinculos.ToList();
        var propios = lista.Where(v => v.IdUsuario == idUsuario).Select(v => v.IdLocal).Distinct().ToList();
        if (rol == Roles.Dueno)
            return propios;

        var duenos = usuarios.Where(u => u.Rol == Roles.Dueno).Select(u => u.Id).ToHashSet();
        var duenosDeMisLocales = lista
            .Where(v => propios.Contains(v.IdLocal) && duenos.Contains(v.IdUsuario))
            .Select(v => v.IdUsuario)
            .ToHashSet();
        if (duenosDeMisLocales.Count == 0)
            return propios;

        return lista
            .Where(v => duenosDeMisLocales.Contains(v.IdUsuario))
            .Select(v => v.IdLocal)
            .Distinct()
            .ToList();
    }

    public static IReadOnlyList<int> IdsVisibles(
        IReadOnlyCollection<int> localesDeLaCuenta,
        IEnumerable<(int IdUsuario, int IdLocal)> asignaciones,
        IEnumerable<(int Id, string Rol)> usuarios)
    {
        var locales = localesDeLaCuenta.ToHashSet();
        var enLaCuenta = asignaciones.Where(a => locales.Contains(a.IdLocal)).Select(a => a.IdUsuario).ToHashSet();
        return usuarios
            .Where(u => enLaCuenta.Contains(u.Id) && (u.Rol == Roles.Empleado || u.Rol == Roles.Socio))
            .Select(u => u.Id)
            .ToList();
    }

    public static bool SesionVigente(int? versionToken, int versionUsuario) =>
        versionToken == versionUsuario;

    public static void CortarSesion(Usuario usuario) => usuario.VersionSesion++;

    public static void CambiarRol(Usuario usuario, string rol)
    {
        if (usuario.Rol == rol) return;
        usuario.Rol = rol;
        CortarSesion(usuario);
    }

    public static void ClaveCambiada(Usuario usuario) => CortarSesion(usuario);

    public static void Desactivar(Usuario usuario)
    {
        if (!usuario.Activo) return;
        usuario.Activo = false;
        CortarSesion(usuario);
    }

    public static bool SacoAlgunLocal(IEnumerable<int> anteriores, IEnumerable<int> nuevos)
    {
        var quedan = nuevos.ToHashSet();
        return anteriores.Any(id => !quedan.Contains(id));
    }
}
