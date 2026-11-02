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
    public class InscribirAlumnoEntrenamientoHandler : IInscribirAlumnoEntrenamientoHandler
    {
        private readonly IEntrenamientoRepository _entrenamientoRepository;

        public InscribirAlumnoEntrenamientoHandler(IEntrenamientoRepository entrenamientoRepository)
        {
            _entrenamientoRepository = entrenamientoRepository;
        }

        public async Task<InscripcionEntrenamientoResponseDto> Handle(InscribirAlumnoEntrenamientoCommand command)
        {
            var entrenamiento = await _entrenamientoRepository.ObtenerPorIdAsync(command.EntrenamientoId);
            if (entrenamiento == null || !entrenamiento.Activo)
            {
                throw new KeyNotFoundException($"No se encontró ningún entrenamiento activo con el ID {command.EntrenamientoId}.");
            }

            var socio = await _entrenamientoRepository.ObtenerSocioPorDniAsync(command.Dni);
            if (socio == null)
            {
                throw new KeyNotFoundException($"No se encontró ningún socio registrado con el DNI {command.Dni}.");
            }

            var yaInscripto = await _entrenamientoRepository.EstaInscriptoAsync(entrenamiento.Id, socio.Id);
            if (yaInscripto)
            {
                throw new InvalidOperationException("El alumno ya se encuentra inscripto en este entrenamiento.");
            }

            // Regla de Negocio: No permitir la inscripción si se alcanzó el cupo límite
            if (entrenamiento.CantidadDeAlumnos >= entrenamiento.CapacidadMaxma)
            {
                throw new InvalidOperationException($"No se puede inscribir al alumno porque se alcanzó el cupo límite del entrenamiento ({entrenamiento.CapacidadMaxma} alumnos).");
            }

            var inscripcion = new Inscripcion_actividad
            {
                Fecha = DateTime.UtcNow,
                estadoInscripcion = EstadoInscripcion.Confirmada,
                SocioId = socio.Id,
                ActividadId = entrenamiento.Id,
                NivelActividad = entrenamiento.nivelActividad
            };

            entrenamiento.CantidadDeAlumnos++;

            var inscripcionCreada = await _entrenamientoRepository.InscribirAlumnoAsync(inscripcion, entrenamiento);

            return new InscripcionEntrenamientoResponseDto
            {
                Id = inscripcionCreada.Id,
                EntrenamientoId = entrenamiento.Id,
                SocioId = socio.Id,
                NombreSocio = $"{socio.Nombre} {socio.Apellido}".Trim(),
                Fecha = inscripcionCreada.Fecha,
                Estado = inscripcionCreada.estadoInscripcion,
                CantidadAlumnosActual = entrenamiento.CantidadDeAlumnos,
                CapacidadMaxima = entrenamiento.CapacidadMaxma
            };
        }
    }
}