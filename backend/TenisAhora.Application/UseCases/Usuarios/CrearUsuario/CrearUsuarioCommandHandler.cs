using TenisAhora.Application.DTOs.Usuarios;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Usuarios.CrearUsuario;

public class CrearUsuarioCommandHandler
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CrearUsuarioCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioResponseDto> Handle(CrearUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        // Validación de regla de negocio: Unicidad de DNI (RF-01)
        var existeDni = await _usuarioRepository.ExisteDniAsync(command.Dni);
        if (existeDni)
        {
            throw new DomainException($"El DNI {command.Dni} ya se encuentra registrado en el sistema.");
        }

        var password = string.IsNullOrWhiteSpace(command.Password) ? "Tenis1234!" : command.Password;
        var passwordHash = _passwordHasher.Hash(password);

        Persona nuevaPersona;

        if (command.Rol == Rol.Socio)
        {
            nuevaPersona = new Socio
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
        }
        else
        {
            nuevaPersona = new Persona
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
                Rol = command.Rol,
                Activo = true
            };
        }

        await _usuarioRepository.AgregarAsync(nuevaPersona, cancellationToken);

        EstadoMembresia? membresia = nuevaPersona is Socio socio ? socio.estadoMembresia : null;

        return new UsuarioResponseDto(
            nuevaPersona.Id,
            nuevaPersona.Nombre,
            nuevaPersona.Apellido,
            nuevaPersona.Dni,
            nuevaPersona.Email,
            nuevaPersona.Telefono,
            nuevaPersona.Direccion,
            nuevaPersona.CodigoPostal,
            nuevaPersona.FechaDeNacimiento,
            nuevaPersona.Rol,
            nuevaPersona.Activo,
            membresia
        );
    }
}
