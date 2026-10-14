using System.ComponentModel.DataAnnotations;
using TenisAhora.Application.DTOs.Usuarios;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Usuarios.ModificarUsuario;

public record ModificarUsuarioCommand(
    [Required(ErrorMessage = "El nombre es obligatorio")]
    string Nombre,

    [Required(ErrorMessage = "El apellido es obligatorio")]
    string Apellido,

    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato de correo electrónico no es válido")]
    string Email,

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    string Telefono,

    [Required(ErrorMessage = "La dirección es obligatoria")]
    string Direccion,

    int CodigoPostal,

    DateTime FechaDeNacimiento,

    EstadoMembresia? EstadoMembresia = null
);

public class ModificarUsuarioCommandHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public ModificarUsuarioCommandHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
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
