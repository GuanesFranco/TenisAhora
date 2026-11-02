using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Infrastructure.Security;
using Xunit;

namespace TenisAhora.UnitTests;

public class AuthSecurityTests
{
    [Fact]
    public void PasswordHasher_DebeHashearYVerificarCorrectamente()
    {
        // 1. Arrange
        var hasher = new PasswordHasher();
        var passwordOriginal = "PasswordSeguro123!";

        // 2. Act
        var hash = hasher.Hash(passwordOriginal);
        var esValida = hasher.Verify(passwordOriginal, hash);
        var esInvalida = hasher.Verify("PasswordIncorrecta", hash);

        // 3. Assert
        Assert.NotNull(hash);
        Assert.NotEqual(passwordOriginal, hash);
        Assert.True(esValida);
        Assert.False(esInvalida);
    }

    [Fact]
    public void JwtProvider_DebeGenerarTokenConClaimsCorrectos()
    {
        // 1. Arrange
        var fakeConfig = new TestConfiguration(new Dictionary<string, string>
        {
            { "Jwt:SecretKey", "SuperSecretaClaveDeSeguridadParaTenisAhoraDeAlMenos32Bytes!" },
            { "Jwt:Issuer", "TenisAhoraApi" },
            { "Jwt:Audience", "TenisAhoraClient" },
            { "Jwt:DurationInMinutes", "60" }
        });

        var jwtProvider = new JwtProvider(fakeConfig);

        var persona = new Persona
        {
            Id = 42,
            Nombre = "Juan",
            Apellido = "Pérez",
            Email = "juan.perez@tenisahora.com",
            Rol = Rol.Administrador
        };

        // 2. Act
        var token = jwtProvider.GenerateToken(persona);

        // 3. Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        // Validar claims contenidos en el token generado
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal("TenisAhoraApi", jwtToken.Issuer);
        Assert.Contains(jwtToken.Audiences, aud => aud == "TenisAhoraClient");

        var claimId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var claimEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var claimRol = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        Assert.Equal("42", claimId);
        Assert.Equal("juan.perez@tenisahora.com", claimEmail);
        Assert.Equal("Administrador", claimRol);
    }

    private class TestConfiguration : IConfiguration
    {
        private readonly Dictionary<string, string> _values;

        public TestConfiguration(Dictionary<string, string> values)
        {
            _values = values;
        }

        public string? this[string key]
        {
            get => _values.TryGetValue(key, out var val) ? val : null;
            set
            {
                if (value != null) _values[key] = value;
            }
        }

        public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();
        public IChangeToken GetReloadToken() => throw new NotImplementedException();
        public IConfigurationSection GetSection(string key) => throw new NotImplementedException();
    }
}
