using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Infrastructure.Security;

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Persona persona)
    {
        var secretKey = _configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("La clave secreta JWT no está configurada en appsettings.");
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var durationMinutes = Convert.ToDouble(_configuration["Jwt:DurationInMinutes"] ?? "120");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, persona.Id.ToString()),
            new Claim(ClaimTypes.Email, persona.Email),
            new Claim(ClaimTypes.GivenName, $"{persona.Nombre} {persona.Apellido}".Trim()),
            new Claim(ClaimTypes.Role, persona.Rol.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(durationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}
