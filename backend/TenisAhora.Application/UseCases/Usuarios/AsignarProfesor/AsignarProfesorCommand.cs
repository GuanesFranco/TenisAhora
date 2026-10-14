using System.ComponentModel.DataAnnotations;
using TenisAhora.Application.DTOs.Usuarios;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Usuarios.AsignarProfesor;

public record AsignarProfesorCommand(
    [Required] int UsuarioDni,
    [Required] int ProfesorId,
    [Required] DateTime FechaHoraInicio,
    [Range(30, 180, ErrorMessage = "La duración debe ser entre 30 y 180 minutos")] int DuracionMinutos = 60,
    NivelActividad Nivel = NivelActividad.Principiante,
    TipoClase TipoDeClase = TipoClase.Tecnica
);

public class AsignarProfesorCommandHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public AsignarProfesorCommandHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<AsignacionResponseDto> Handle(AsignarProfesorCommand command, CancellationToken cancellationToken = default)
    {
        var persona = await _usuarioRepository.ObtenerPorDniAsync(command.UsuarioDni);
        if (persona is null)
        {
            throw new DomainException($"No se encontró al usuario con DNI {command.UsuarioDni}.");
        }

        if (persona is not Socio socio)
        {
            throw new DomainException($"El usuario con DNI {command.UsuarioDni} no está registrado como Socio para tomar clases.");
        }

        var profesor = await _usuarioRepository.ObtenerProfesorPorIdAsync(command.ProfesorId);
        if (profesor is null || !profesor.Activo)
        {
            throw new DomainException($"El profesor seleccionado con ID {command.ProfesorId} no existe o no se encuentra activo.");
        }

        var horaFin = command.FechaHoraInicio.AddMinutes(command.DuracionMinutos);

        var clase = new Clase
        {
            HoraInicio = command.FechaHoraInicio,
            HoraFin = horaFin,
            Duracion = command.DuracionMinutos,
            CapacidadMaxma = 1,
            CantidadDeAlumnos = 1,
            ProfesorId = profesor.Id,
            nivelActividad = command.Nivel,
            TipoDeClase = command.TipoDeClase,
            Activo = true
        };

        var inscripcion = new Inscripcion_actividad
        {
            SocioId = socio.Id,
            Fecha = DateTime.Now,
            estadoInscripcion = EstadoInscripcion.Confirmada,
            NivelActividad = command.Nivel,
            Actividad = clase
        };

        await _usuarioRepository.RegistrarActividadParticularAsync(clase, inscripcion, cancellationToken);

        return new AsignacionResponseDto(
            clase.Id,
            socio.Id,
            $"{socio.Nombre} {socio.Apellido}",
            profesor.Id,
            $"{profesor.Nombre} {profesor.Apellido}",
            "Profesor",
            clase.HoraInicio,
            clase.HoraFin,
            $"Profesor {profesor.Nombre} {profesor.Apellido} asignado exitosamente al socio {socio.Nombre} {socio.Apellido}."
        );
    }
}
