using TenisAhora.Application.DTOs.Auth;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Auth.Login;

public class LoginCommandHandler
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> Handle(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new DomainException("El email y la contraseña son obligatorios.");
        }

        var persona = await _usuarioRepository.ObtenerPorEmailAsync(request.Email);
        if (persona is null)
        {
            throw new DomainException("Credenciales inválidas.");
        }

        if (!persona.Activo)
        {
            throw new DomainException("La cuenta de usuario se encuentra inactiva.");
        }

        var esPasswordValida = _passwordHasher.Verify(request.Password, persona.PasswordHash);
        if (!esPasswordValida)
        {
            throw new DomainException("Credenciales inválidas.");
        }

        var token = _jwtProvider.GenerateToken(persona);

        return new AuthResponse(
            Token: token,
            UsuarioId: persona.Id,
            Nombre: persona.Nombre,
            Apellido: persona.Apellido,
            Email: persona.Email,
            Rol: persona.Rol.ToString()
        );
    }
}
