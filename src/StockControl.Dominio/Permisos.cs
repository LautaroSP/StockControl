namespace StockControl.Dominio;

public static class Permisos
{
    public static bool PuedeCrearProducto(string rol) =>
        EsDuenoOperativo(rol);

    public static bool PuedeEditarProducto(string rol) =>
        EsDuenoOperativo(rol);

    public static bool PuedeModificarPrecioEmpleado(string rol, bool habilitado) =>
        rol == Roles.Empleado && habilitado;

    public static bool PuedeCobrarAlCosto(string rol) =>
        EsDuenoOperativo(rol);

    public static bool PuedeAnularVenta(string rol) =>
        EsDuenoOperativo(rol);

    public static bool PuedeVerCostoEnInforme(string rol) =>
        EsDuenoOperativo(rol);

    public static bool PuedeAdministrarGrupos(string rol) =>
        EsDuenoOperativo(rol);

    public static bool PuedeConfigurarCajas(string rol) =>
        rol is Roles.Dueno or Roles.Socio;

    public static bool PuedeConfigurarLocal(string rol) =>
        rol is Roles.Dueno or Roles.Socio;

    public static bool PuedeAdministrarMetodosPago(string rol) =>
        EsDuenoOperativo(rol);

    public static bool EsDuenoOperativo(string rol) =>
        rol is Roles.Dueno or Roles.Socio or Roles.Admin;

    public static bool PuedeAdministrarUsuarios(string rol) =>
        EsDuenoOperativo(rol);

    public static bool PuedeAsignarSocio(string rol) =>
        rol is Roles.Dueno or Roles.Admin;

    public static bool EsRolGestionable(string rol) =>
        rol is Roles.Empleado or Roles.Socio;
}
