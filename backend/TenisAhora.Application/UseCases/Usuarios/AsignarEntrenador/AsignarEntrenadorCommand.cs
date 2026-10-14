using System.ComponentModel.DataAnnotations;
using TenisAhora.Application.DTOs.Usuarios;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Usuarios.AsignarEntrenador;

public record AsignarEntrenadorCommand(
    [Required] int UsuarioDni,
    [Required] int EntrenadorId,
    [Required] DateTime FechaHoraInicio,
    [Range(30, 180, ErrorMessage = "La duración debe ser entre 30 y 180 minutos")] int DuracionMinutos = 60,
    NivelActividad Nivel = NivelActividad.Principiante,
    ObjetivoEntrenamiento Objetivo = ObjetivoEntrenamiento.Tecnica
);

public class AsignarEntrenadorCommandHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public AsignarEntrenadorCommandHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<AsignacionResponseDto> Handle(AsignarEntrenadorCommand command, CancellationToken cancellationToken = default)
    {
        var persona = await _usuarioRepository.ObtenerPorDniAsync(command.UsuarioDni);
        if (persona is null)
        {
            throw new DomainException($"No se encontró al usuario con DNI {command.UsuarioDni}.");
        }

        if (persona is not Socio socio)
        {
            throw new DomainException($"El usuario con DNI {command.UsuarioDni} no está registrado como Socio para entrenamientos.");
        }

        var entrenador = await _usuarioRepository.ObtenerEntrenadorPorIdAsync(command.EntrenadorId);
        if (entrenador is null || !entrenador.Activo)
        {
            throw new DomainException($"El entrenador seleccionado con ID {command.EntrenadorId} no existe o no se encuentra activo.");
        }

        var horaFin = command.FechaHoraInicio.AddMinutes(command.DuracionMinutos);

        var entrenamiento = new Entrenamiento
        {
            HoraInicio = command.FechaHoraInicio,
            HoraFin = horaFin,
            Duracion = command.DuracionMinutos,
            CapacidadMaxma = 1,
            CantidadDeAlumnos = 1,
            EntrenadorId = entrenador.Id,
            nivelActividad = command.Nivel,
            objetivoEntrenamiento = command.Objetivo,
            Activo = true
        };

        var inscripcion = new Inscripcion_actividad
        {
            SocioId = socio.Id,
            Fecha = DateTime.Now,
            estadoInscripcion = EstadoInscripcion.Confirmada,
            NivelActividad = command.Nivel,
            Actividad = entrenamiento
        };

        await _usuarioRepository.RegistrarActividadParticularAsync(entrenamiento, inscripcion, cancellationToken);

        return new AsignacionResponseDto(
            entrenamiento.Id,
            socio.Id,
            $"{socio.Nombre} {socio.Apellido}",
            entrenador.Id,
            $"{entrenador.Nombre} {entrenador.Apellido}",
            "Entrenador",
            entrenamiento.HoraInicio,
            entrenamiento.HoraFin,
            $"Entrenador {entrenador.Nombre} {entrenador.Apellido} asignado exitosamente al socio {socio.Nombre} {socio.Apellido}."
        );
    }
}
