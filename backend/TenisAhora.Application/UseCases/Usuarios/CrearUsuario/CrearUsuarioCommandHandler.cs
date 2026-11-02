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
                Activo = false, // Inactivo hasta que el administrador apruebe la certificación
                AdministradorId = adminId,
                especialidad = !string.IsNullOrWhiteSpace(command.Especialidad) 
                    ? command.Especialidad.Trim() 
                    : (!string.IsNullOrWhiteSpace(command.TituloHabilitante) ? command.TituloHabilitante.Trim() : "Profesor de Tenis"),
                antiguedad = command.Antiguedad ?? 1,
                TipoDeEnsenanza = TipoClase.Tecnica
            };

            prof.certificacionDeportiva = new CertificacionDeportiva
            {
                certificacionDeportiva = false,
                FechaEmision = DateTime.Today,
                NombreCertificacion = !string.IsNullOrWhiteSpace(command.TituloHabilitante) ? command.TituloHabilitante.Trim() : "Profesor de Tenis",
                EnteEmisor = string.IsNullOrWhiteSpace(command.EnteEmisor) ? "Asociación Argentina de Tenis" : command.EnteEmisor.Trim(),
                EstadoCertificacion = EstadoCertificacion.Pendiente
            };

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
                Activo = false, // Inactivo hasta que el administrador apruebe la certificación
                AdministradorId = adminId,
                especialidad = !string.IsNullOrWhiteSpace(command.Especialidad) 
                    ? command.Especialidad.Trim() 
                    : (!string.IsNullOrWhiteSpace(command.TituloHabilitante) ? command.TituloHabilitante.Trim() : "Entrenador de Tenis"),
                antiguedad = command.Antiguedad ?? 1,
                Objetivo = ObjetivoEntrenamiento.Tecnica
            };

            ent.certificacionDeportiva = new CertificacionDeportiva
            {
                certificacionDeportiva = false,
                FechaEmision = DateTime.Today,
                NombreCertificacion = !string.IsNullOrWhiteSpace(command.TituloHabilitante) ? command.TituloHabilitante.Trim() : "Entrenador de Tenis",
                EnteEmisor = string.IsNullOrWhiteSpace(command.EnteEmisor) ? "Asociación Argentina de Tenis" : command.EnteEmisor.Trim(),
                EstadoCertificacion = EstadoCertificacion.Pendiente
            };

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
        int? certId = null;
        string? titulo = null;
        string? ente = null;
        EstadoCertificacion? estadoCert = null;

        if (nuevaPersona is Instructor instructor && instructor.certificacionDeportiva != null)
        {
            certId = instructor.certificacionDeportiva.Id;
            titulo = instructor.certificacionDeportiva.NombreCertificacion;
            ente = instructor.certificacionDeportiva.EnteEmisor;
            estadoCert = instructor.certificacionDeportiva.EstadoCertificacion;
        }

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
            membresia,
            certId,
            titulo,
            ente,
            estadoCert
        );
    }
}
