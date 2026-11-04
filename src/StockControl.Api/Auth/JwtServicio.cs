using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StockControl.Dominio;

namespace StockControl.Api.Auth;

public class JwtServicio
{
    private readonly string _clave;
    private readonly string _emisor;
    private readonly string _audiencia;

    public JwtServicio(IConfiguration config)
    {
        (_clave, _emisor, _audiencia) = Leer(config);
    }

    public static (string Clave, string Emisor, string Audiencia) Leer(IConfiguration config)
    {
        var clave = ExigirClave(config["Jwt:Clave"]);
        var emisor = ExigirValor(config["Jwt:Emisor"], "Jwt:Emisor");
        var audiencia = ExigirValor(config["Jwt:Audiencia"], "Jwt:Audiencia");
        return (clave, emisor, audiencia);
    }

    public static string ExigirClave(string? clave)
    {
        if (string.IsNullOrEmpty(clave) || clave.Length < 32)
            throw new InvalidOperationException("Jwt:Clave debe tener al menos 32 caracteres.");
        return clave;
    }

    public string Emitir(Usuario usuario, int? idLocal, int? nroCaja = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new("rol", usuario.Rol),
            new("usuario", usuario.NombreUsuario),
            new("nombre", string.IsNullOrWhiteSpace(usuario.Nombre) ? usuario.NombreUsuario : usuario.Nombre),
            new("sesion", usuario.VersionSesion.ToString())
        };
        if (idLocal.HasValue)
            claims.Add(new Claim("localId", idLocal.Value.ToString()));
        if (nroCaja.HasValue)
            claims.Add(new Claim("nroCaja", nroCaja.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_clave));
        var token = new JwtSecurityToken(
            issuer: _emisor,
            audience: _audiencia,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    static string ExigirValor(string? valor, string nombre)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new InvalidOperationException($"Falta {nombre}.");
        return valor;
    }
}
