using System.ComponentModel.DataAnnotations;
using TenisAhora.Application.DTOs.Auth;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Auth.Registro;

public record RegistrarSocioCommand(
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    string Nombre,

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
    string Apellido,

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [Range(1000000, 99999999, ErrorMessage = "El DNI debe tener entre 7 y 8 dígitos")]
    int Dni,

    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato de correo no es válido")]
    string Email,

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    string Telefono,

    [Required(ErrorMessage = "La dirección es obligatoria")]
    string Direccion,

    int CodigoPostal,

    DateTime FechaDeNacimiento,

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    string Password
);

public class RegistrarSocioCommandHandler
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public RegistrarSocioCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> Handle(RegistrarSocioCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Validar unicidad de DNI
        if (await _usuarioRepository.ExisteDniAsync(command.Dni))
        {
            throw new DomainException($"El DNI {command.Dni} ya se encuentra registrado.");
        }

        // 2. Validar unicidad de Email
        if (await _usuarioRepository.ExisteEmailAsync(command.Email))
        {
            throw new DomainException($"El correo electrónico {command.Email} ya se encuentra registrado.");
        }

        // 3. Hashear password
        var passwordHash = _passwordHasher.Hash(command.Password);

        // 4. Instanciar nueva entidad Socio
        var nuevoSocio = new Socio
        {
            Nombre = command.Nombre.Trim(),
            Apellido = command.Apellido.Trim(),
            Dni = command.Dni,
            Email = command.Email.Trim().ToLowerInvariant(),
            Telefono = command.Telefono.Trim(),
            Direccion = command.Direccion.Trim(),
            CodigoPostal = command.CodigoPostal,
            FechaDeNacimiento = command.FechaDeNacimiento,
            PasswordHash = passwordHash,
            Rol = Rol.Socio,
            Activo = true,
            estadoMembresia = EstadoMembresia.Activa
        };

        await _usuarioRepository.AgregarAsync(nuevoSocio, cancellationToken);

        // 5. Generar token JWT automático para iniciar sesión al registrarse
        var token = _jwtProvider.GenerateToken(nuevoSocio);

        return new AuthResponse(
            Token: token,
            UsuarioId: nuevoSocio.Id,
            Nombre: nuevoSocio.Nombre,
            Apellido: nuevoSocio.Apellido,
            Email: nuevoSocio.Email,
            Rol: nuevoSocio.Rol.ToString()
        );
    }
}
