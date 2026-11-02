using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Entrenamientos.Commands;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Entrenamientos.Handlers
{
    public class AsignarEntrenadorParticularHandler : IAsignarEntrenadorParticularHandler
    {
        private readonly IEntrenamientoRepository _entrenamientoRepository;

        public AsignarEntrenadorParticularHandler(IEntrenamientoRepository entrenamientoRepository)
        {
            _entrenamientoRepository = entrenamientoRepository;
        }

        public async Task<EntrenamientoParticularResponseDto> Handle(AsignarEntrenadorParticularCommand command)
        {
            var socio = await _entrenamientoRepository.ObtenerSocioPorDniAsync(command.Dni);
            if (socio == null)
            {
                throw new KeyNotFoundException($"No se encontró ningún socio registrado con el DNI {command.Dni}.");
            }

            var entrenador = await _entrenamientoRepository.ObtenerEntrenadorPorIdAsync(command.EntrenadorId);
            if (entrenador == null || !entrenador.Activo)
            {
                throw new KeyNotFoundException($"No se encontró ningún entrenador activo con el ID {command.EntrenadorId}.");
            }

            var nombreEntrenamiento = !string.IsNullOrWhiteSpace(command.Nombre)
                ? command.Nombre
                : $"Entrenamiento Particular - {socio.Nombre} {socio.Apellido}";

            var entrenamiento = new Entrenamiento
            {
                Nombre = nombreEntrenamiento,
                objetivoEntrenamiento = default,
                HoraInicio = command.HoraInicio,
                HoraFin = command.HoraFin,
                CapacidadMaxma = 1,
                CantidadDeAlumnos = 1,
                Duracion = command.Duracion,
                EntrenadorId = entrenador.Id,
                nivelActividad = NivelActividad.Principiante,
                Activo = true
            };

            var inscripcion = new Inscripcion_actividad
            {
                Fecha = DateTime.UtcNow,
                estadoInscripcion = EstadoInscripcion.Confirmada,
                SocioId = socio.Id,
                NivelActividad = entrenamiento.nivelActividad
            };

            inscripcion.Actividad = entrenamiento;

            var guardado = await _entrenamientoRepository.RegistrarEntrenamientoParticularAsync(entrenamiento, inscripcion);

            return new EntrenamientoParticularResponseDto
            {
                EntrenamientoId = guardado.Id,
                Nombre = guardado.Nombre,
                EntrenadorId = entrenador.Id,
                NombreEntrenador = $"{entrenador.Nombre} {entrenador.Apellido}".Trim(),
                SocioId = socio.Id,
                NombreSocio = $"{socio.Nombre} {socio.Apellido}".Trim(),
                HoraInicio = guardado.HoraInicio,
                HoraFin = guardado.HoraFin,
                Duracion = guardado.Duracion,
                InscripcionId = inscripcion.Id
            };
        }
    }
}