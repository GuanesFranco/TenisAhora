using System;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Entrenamientos.Commands;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.UseCases.Entrenamientos.Handlers
{
    public class CreateEntrenamientoHandler : ICreateEntrenamientoHandler
    {
        private readonly IEntrenamientoRepository _entrenamientoRepository;

        public CreateEntrenamientoHandler(IEntrenamientoRepository entrenamientoRepository)
        {
            _entrenamientoRepository = entrenamientoRepository;
        }

        public async Task<EntrenamientoResponseDto> Handle(CreateEntrenamientoCommand command)
        {
            // Regla de Negocio RD-03 / RF-49: El cupo no puede superar los 10 alumnos
            if (command.CapacidadMaxma > 10)
            {
                throw new InvalidOperationException("El cupo máximo del entrenamiento no puede ser superior a 10 alumnos (RD-03).");
            }

            if (command.CapacidadMaxma <= 0)
            {
                throw new InvalidOperationException("El cupo del entrenamiento debe ser mayor a 0.");
            }

            var entrenamiento = new Entrenamiento
            {
                Nombre = command.Nombre,
                objetivoEntrenamiento = command.ObjetivoEntrenamiento,
                HoraInicio = command.HoraInicio,
                HoraFin = command.HoraFin,
                CapacidadMaxma = command.CapacidadMaxma,
                CantidadDeAlumnos = 0,
                Duracion = command.Duracion,
                EntrenadorId = command.EntrenadorId,
                nivelActividad = command.NivelActividad,
                Activo = true
            };

            var creado = await _entrenamientoRepository.CrearAsync(entrenamiento);

            return new EntrenamientoResponseDto
            {
                Id = creado.Id,
                Nombre = creado.Nombre,
                ObjetivoEntrenamiento = creado.objetivoEntrenamiento,
                HoraInicio = creado.HoraInicio,
                HoraFin = creado.HoraFin,
                CapacidadMaxma = creado.CapacidadMaxma,
                CantidadDeAlumnos = creado.CantidadDeAlumnos,
                Duracion = creado.Duracion,
                EntrenadorId = creado.EntrenadorId,
                Activo = creado.Activo,
                NivelActividad = creado.nivelActividad
            };
        }
    }
}