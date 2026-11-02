using System.ComponentModel.DataAnnotations;
using TenisAhora.Application.DTOs.Usuarios;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Usuarios.ModificarUsuario;

public record ModificarUsuarioCommand(
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre debe contener únicamente letras y espacios.")]
    string Nombre,

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido debe contener únicamente letras y espacios.")]
    string Apellido,

    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato de correo electrónico no es válido")]
    string Email,

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [StringLength(15, MinimumLength = 8, ErrorMessage = "El teléfono debe tener entre 8 y 15 dígitos.")]
    [RegularExpression(@"^[0-9]{8,15}$", ErrorMessage = "El teléfono debe contener entre 8 y 15 dígitos numéricos.")]
    string Telefono,

    [Required(ErrorMessage = "La dirección es obligatoria")]
    string Direccion,

    int CodigoPostal,

    DateTime FechaDeNacimiento,

    EstadoMembresia? EstadoMembresia = null,

    string? Password = null
);

public class ModificarUsuarioCommandHandler
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ModificarUsuarioCommandHandler(IUsuarioRepository usuarioRepository, IPasswordHasher passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioResponseDto> Handle(int dni, ModificarUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        var persona = await _usuarioRepository.ObtenerPorDniAsync(dni);
        if (persona is null)
        {
            throw new DomainException($"No se encontró ningún usuario con DNI {dni} para modificar.");
        }

        persona.Nombre = command.Nombre.Trim();
        persona.Apellido = command.Apellido.Trim();
        persona.Email = command.Email.Trim().ToLowerInvariant();
        persona.Telefono = command.Telefono.Trim();
        persona.Direccion = command.Direccion.Trim();
        persona.CodigoPostal = command.CodigoPostal;
        persona.FechaDeNacimiento = command.FechaDeNacimiento;

        if (!string.IsNullOrWhiteSpace(command.Password))
        {
            persona.PasswordHash = _passwordHasher.Hash(command.Password.Trim());
        }

        if (persona is Socio socio && command.EstadoMembresia.HasValue)
        {
            socio.estadoMembresia = command.EstadoMembresia.Value;
        }

        await _usuarioRepository.ActualizarAsync(persona, cancellationToken);

        EstadoMembresia? membresia = persona is Socio s ? s.estadoMembresia : null;

        return new UsuarioResponseDto(
            persona.Id,
            persona.Nombre,
            persona.Apellido,
            persona.Dni,
            persona.Email,
            persona.Telefono,
            persona.Direccion,
            persona.CodigoPostal,
            persona.FechaDeNacimiento,
            persona.Rol,
            persona.Activo,
            membresia
        );
    }
}
