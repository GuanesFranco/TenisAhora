using System;
using System.Threading;
using System.Threading.Tasks;
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

        var existeEmail = await _usuarioRepository.ExisteEmailAsync(command.Email);
        if (existeEmail)
        {
            throw new DomainException($"El correo electrónico {command.Email} ya se encuentra registrado en el sistema.");
        }

        var password = string.IsNullOrWhiteSpace(command.Password) ? "Tenis1234!" : command.Password;
        var passwordHash = _passwordHasher.Hash(password);
        var fechaNac = command.FechaDeNacimiento ?? new DateTime(1995, 1, 1);
        var cp = command.CodigoPostal > 0 ? command.CodigoPostal : 1000;

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
                CodigoPostal = cp,
                FechaDeNacimiento = fechaNac,
                PasswordHash = passwordHash,
                Rol = Rol.Socio,
                Activo = true,
                estadoMembresia = EstadoMembresia.Activa
            };
        }
        else if (command.Rol == Rol.Profesor)
        {
            var adminId = await _usuarioRepository.ObtenerPrimerAdminIdAsync() ?? 1;
            var prof = new Profesor
            {
                Nombre = command.Nombre.Trim(),
                Apellido = command.Apellido.Trim(),
                Dni = command.Dni,
                Email = command.Email.Trim().ToLowerInvariant(),
                Telefono = command.Telefono.Trim(),
                Direccion = command.Direccion.Trim(),
                CodigoPostal = cp,
                FechaDeNacimiento = fechaNac,
                PasswordHash = passwordHash,
                Rol = Rol.Profesor,
                Activo = true,
                AdministradorId = adminId,
                especialidad = !string.IsNullOrWhiteSpace(command.Especialidad) 
                    ? command.Especialidad.Trim() 
                    : (!string.IsNullOrWhiteSpace(command.TituloHabilitante) ? command.TituloHabilitante.Trim() : "Profesor de Tenis"),
                antiguedad = command.Antiguedad ?? 1,
                TipoDeEnsenanza = TipoClase.Tecnica
            };

            if (!string.IsNullOrWhiteSpace(command.TituloHabilitante))
            {
                prof.certificacionDeportiva = new CertificacionDeportiva
                {
                    certificacionDeportiva = true,
                    FechaEmision = DateTime.Today,
                    NombreCertificacion = command.TituloHabilitante.Trim(),
                    EnteEmisor = string.IsNullOrWhiteSpace(command.EnteEmisor) ? "Asociación Argentina de Tenis" : command.EnteEmisor.Trim(),
                    EstadoCertificacion = EstadoCertificacion.Aprobada
                };
            }

            nuevaPersona = prof;
        }
        else if (command.Rol == Rol.Entrenador)
        {
            var adminId = await _usuarioRepository.ObtenerPrimerAdminIdAsync() ?? 1;
            var ent = new Entrenador
            {
                Nombre = command.Nombre.Trim(),
                Apellido = command.Apellido.Trim(),
                Dni = command.Dni,
                Email = command.Email.Trim().ToLowerInvariant(),
                Telefono = command.Telefono.Trim(),
                Direccion = command.Direccion.Trim(),
                CodigoPostal = cp,
                FechaDeNacimiento = fechaNac,
                PasswordHash = passwordHash,
                Rol = Rol.Entrenador,
                Activo = true,
                AdministradorId = adminId,
                especialidad = !string.IsNullOrWhiteSpace(command.Especialidad) 
                    ? command.Especialidad.Trim() 
                    : (!string.IsNullOrWhiteSpace(command.TituloHabilitante) ? command.TituloHabilitante.Trim() : "Entrenador de Tenis"),
                antiguedad = command.Antiguedad ?? 1,
                Objetivo = ObjetivoEntrenamiento.Tecnica
            };

            if (!string.IsNullOrWhiteSpace(command.TituloHabilitante))
            {
                ent.certificacionDeportiva = new CertificacionDeportiva
                {
                    certificacionDeportiva = true,
                    FechaEmision = DateTime.Today,
                    NombreCertificacion = command.TituloHabilitante.Trim(),
                    EnteEmisor = string.IsNullOrWhiteSpace(command.EnteEmisor) ? "Asociación Argentina de Tenis" : command.EnteEmisor.Trim(),
                    EstadoCertificacion = EstadoCertificacion.Aprobada
                };
            }

            nuevaPersona = ent;
        }
        else if (command.Rol == Rol.Administrador)
        {
            nuevaPersona = new Administrador
            {
                Nombre = command.Nombre.Trim(),
                Apellido = command.Apellido.Trim(),
                Dni = command.Dni,
                Email = command.Email.Trim().ToLowerInvariant(),
                Telefono = command.Telefono.Trim(),
                Direccion = command.Direccion.Trim(),
                CodigoPostal = cp,
                FechaDeNacimiento = fechaNac,
                PasswordHash = passwordHash,
                Rol = Rol.Administrador,
                Activo = true
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
                CodigoPostal = cp,
                FechaDeNacimiento = fechaNac,
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
