using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.DTOs.Auth;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Auth.Login;
using TenisAhora.Application.UseCases.Auth.Registro;

namespace TenisAhora.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly LoginCommandHandler _loginHandler;
    private readonly RegistrarSocioCommandHandler _registrarHandler;
    private readonly IUsuarioRepository _usuarioRepository;

    public AuthController(
        LoginCommandHandler loginHandler,
        RegistrarSocioCommandHandler registrarHandler,
        IUsuarioRepository usuarioRepository)
    {
        _loginHandler = loginHandler;
        _registrarHandler = registrarHandler;
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Inicia sesión con correo electrónico y contraseña
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _loginHandler.Handle(request);
        return Ok(response);
    }

    /// <summary>
    /// Registra un nuevo socio y genera su sesión JWT de manera inmediata
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegistrarSocioCommand command)
    {
        var response = await _registrarHandler.Handle(command);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Retorna los datos del usuario autenticado actual desde el token JWT validando que la cuenta esté activa
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> ObtenerUsuarioActual()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized();
        }

        var persona = await _usuarioRepository.ObtenerPorEmailAsync(email);
        if (persona is null || !persona.Activo)
        {
            return Unauthorized(new { mensaje = "La cuenta de usuario se encuentra inactiva o ha sido dada de baja." });
        }

        return Ok(new
        {
            Id = persona.Id.ToString(),
            Email = persona.Email,
            Rol = persona.Rol.ToString(),
            NombreCompleto = $"{persona.Nombre} {persona.Apellido}".Trim(),
            Dni = persona.Dni
        });
    }
}
