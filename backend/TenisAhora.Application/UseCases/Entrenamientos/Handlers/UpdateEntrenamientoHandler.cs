using System;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Entrenamientos.Commands;

namespace TenisAhora.Application.UseCases.Entrenamientos.Handlers
{
    public class UpdateEntrenamientoHandler : IUpdateEntrenamientoHandler
    {
        private readonly IEntrenamientoRepository _entrenamientoRepository;

        public UpdateEntrenamientoHandler(IEntrenamientoRepository entrenamientoRepository)
        {
            _entrenamientoRepository = entrenamientoRepository;
        }

        public async Task<EntrenamientoResponseDto?> Handle(int id, UpdateEntrenamientoCommand command)
        {
            var e = await _entrenamientoRepository.ObtenerPorIdAsync(id);
            if (e == null) return null;

            if (command.CapacidadMaxma > 10)
            {
                throw new InvalidOperationException("El cupo máximo del entrenamiento no puede ser superior a 10 alumnos (RD-03).");
            }

            if (command.CapacidadMaxma < e.CantidadDeAlumnos)
            {
                throw new InvalidOperationException($"El cupo no puede ser menor a la cantidad de alumnos ya inscriptos ({e.CantidadDeAlumnos}).");
            }

            if (!command.EntrenadorId.HasValue || command.EntrenadorId.Value <= 0)
            {
                throw new InvalidOperationException("Es obligatorio asignar un entrenador para el entrenamiento.");
            }

            var entrenamientosExistentes = await _entrenamientoRepository.ObtenerTodosAsync();
            var tieneSuperposicion = System.Linq.Enumerable.Any(entrenamientosExistentes, ex =>
                ex.Activo && ex.Id != id && ex.EntrenadorId == command.EntrenadorId &&
                ex.HoraInicio < command.HoraFin && ex.HoraFin > command.HoraInicio);
            if (tieneSuperposicion)
            {
                throw new InvalidOperationException("El entrenador seleccionado ya tiene asignado otro entrenamiento en esa misma franja horaria.");
            }

            e.Nombre = command.Nombre;
            e.objetivoEntrenamiento = command.ObjetivoEntrenamiento;
            e.HoraInicio = command.HoraInicio;
            e.HoraFin = command.HoraFin;
            e.CapacidadMaxma = command.CapacidadMaxma;
            e.Duracion = command.Duracion;
            e.EntrenadorId = command.EntrenadorId;
            e.nivelActividad = command.NivelActividad;

            await _entrenamientoRepository.ModificarAsync(e);

            return new EntrenamientoResponseDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                ObjetivoEntrenamiento = e.objetivoEntrenamiento,
                HoraInicio = e.HoraInicio,
                HoraFin = e.HoraFin,
                CapacidadMaxma = e.CapacidadMaxma,
                CantidadDeAlumnos = e.CantidadDeAlumnos,
                Duracion = e.Duracion,
                EntrenadorId = e.EntrenadorId,
                Activo = e.Activo,
                NivelActividad = e.nivelActividad
            };
        }
    }
}