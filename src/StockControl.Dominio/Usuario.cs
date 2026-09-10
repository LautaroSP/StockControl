namespace StockControl.Dominio;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string NombreUsuario { get; set; } = "";
    public string HashClave { get; set; } = "";
    public string Rol { get; set; } = Roles.Empleado;
    public bool Activo { get; set; } = true;
}
