using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.DTOs.Auth;
using TenisAhora.Application.UseCases.Auth.Login;
using TenisAhora.Application.UseCases.Auth.Registro;

namespace TenisAhora.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly LoginCommandHandler _loginHandler;
    private readonly RegistrarSocioCommandHandler _registrarHandler;

    public AuthController(
        LoginCommandHandler loginHandler,
        RegistrarSocioCommandHandler registrarHandler)
    {
        _loginHandler = loginHandler;
        _registrarHandler = registrarHandler;
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
    /// Retorna los datos del usuario autenticado actual desde el token JWT
    /// </summary>
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
