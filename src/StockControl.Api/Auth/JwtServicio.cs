using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StockControl.Dominio;

namespace StockControl.Api.Auth;

public class JwtServicio
{
    private readonly string _clave;

    public JwtServicio(IConfiguration config)
    {
        _clave = config["Jwt:Clave"]
                 ?? throw new InvalidOperationException("Falta Jwt:Clave en configuración.");
    }

    public string Emitir(Usuario usuario, int? idLocal)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new("rol", usuario.Rol),
            new("nombre", usuario.NombreUsuario)
        };
        if (idLocal.HasValue)
            claims.Add(new Claim("localId", idLocal.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_clave));
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
