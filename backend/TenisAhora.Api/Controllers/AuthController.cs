using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TenisAhora.Application.DTOs.Auth;
using TenisAhora.Application.Interfaces;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public AuthController(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("El email y la contraseña son requeridos.");
        }

        var persona = await _context.Personas
            .FirstOrDefaultAsync(p => p.Email == request.Email);

        if (persona is null)
        {
            return Unauthorized("Credenciales inválidas.");
        }

        var esPasswordValida = _passwordHasher.Verify(request.Password, persona.PasswordHash);
        if (!esPasswordValida)
        {
            return Unauthorized("Credenciales inválidas.");
        }

        var token = _jwtProvider.GenerateToken(persona);

        return Ok(new AuthResponse(
            Token: token,
            UsuarioId: persona.Id,
            Nombre: persona.Nombre,
            Apellido: persona.Apellido,
            Email: persona.Email,
            Rol: persona.Rol.ToString()
        ));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult ObtenerUsuarioActual()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var rol = User.FindFirstValue(ClaimTypes.Role);
        var nombre = User.FindFirstValue(ClaimTypes.GivenName);

        return Ok(new
        {
            Id = id,
            Email = email,
            Rol = rol,
            NombreCompleto = nombre
        });
    }
}
