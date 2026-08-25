namespace StockControl.Dominio;

public static class Permisos
{
    public static bool PuedeCrearProducto(string rol) =>
        rol is Roles.Dueno or Roles.Admin;

    public static bool PuedeEditarProducto(string rol) =>
        rol is Roles.Dueno or Roles.Admin;

    public static bool PuedeCobrarAlCosto(string rol) =>
        rol is Roles.Dueno or Roles.Admin;

    public static bool PuedeAnularVenta(string rol) =>
        rol is Roles.Dueno or Roles.Admin;

    public static bool PuedeVerCostoEnInforme(string rol) =>
        rol is Roles.Dueno or Roles.Admin;
}
