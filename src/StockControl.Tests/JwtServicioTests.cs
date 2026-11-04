using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using StockControl.Api.Auth;
using StockControl.Dominio;

namespace StockControl.Tests;

public class JwtServicioTests
{
    [Fact]
    public void Una_clave_corta_no_sirve()
    {
        var error = Assert.Throws<InvalidOperationException>(() => JwtServicio.ExigirClave("corta"));

        Assert.Equal("Jwt:Clave debe tener al menos 32 caracteres.", error.Message);
    }

    [Fact]
    public void El_token_lleva_la_version_el_emisor_y_la_audiencia()
    {
        var jwt = new JwtServicio(new ConfigJwt(new Dictionary<string, string?>
        {
            ["Jwt:Clave"] = new string('a', 32),
            ["Jwt:Emisor"] = "StockControl",
            ["Jwt:Audiencia"] = "StockControl"
        }));
        var usuario = new Usuario { Id = 7, NombreUsuario = "ana", Rol = Roles.Empleado, VersionSesion = 4 };

        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt.Emitir(usuario, null));

        Assert.Equal("StockControl", token.Issuer);
        Assert.Equal("StockControl", token.Audiences.Single());
        Assert.Equal("4", token.Claims.Single(c => c.Type == "sesion").Value);
    }

    sealed class ConfigJwt : IConfiguration
    {
        readonly Dictionary<string, string?> _valores;

        public ConfigJwt(Dictionary<string, string?> valores) => _valores = valores;

        public string? this[string key]
        {
            get => _valores.TryGetValue(key, out var valor) ? valor : null;
            set => _valores[key] = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => [];

        public IChangeToken GetReloadToken() => TokenVacio.Instancia;

        public IConfigurationSection GetSection(string key) => new SeccionVacia(key);

        sealed class SeccionVacia : IConfigurationSection
        {
            public SeccionVacia(string key) => Key = key;

            public string? this[string key]
            {
                get => null;
                set { }
            }

            public string Key { get; }
            public string Path => Key;
            public string? Value { get; set; }
            public IEnumerable<IConfigurationSection> GetChildren() => [];
            public IChangeToken GetReloadToken() => TokenVacio.Instancia;
            public IConfigurationSection GetSection(string key) => new SeccionVacia(key);
        }
    }

    sealed class TokenVacio : IChangeToken
    {
        public static readonly TokenVacio Instancia = new();
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => Vacio.Instancia;

        sealed class Vacio : IDisposable
        {
            public static readonly Vacio Instancia = new();
            public void Dispose() { }
        }
    }
}
