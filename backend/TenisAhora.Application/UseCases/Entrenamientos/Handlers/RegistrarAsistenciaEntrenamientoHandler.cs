using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Entrenamientos.Commands;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.UseCases.Entrenamientos.Handlers
{
    public class RegistrarAsistenciaEntrenamientoHandler : IRegistrarAsistenciaEntrenamientoHandler
    {
        private readonly IEntrenamientoRepository _entrenamientoRepository;

        public RegistrarAsistenciaEntrenamientoHandler(IEntrenamientoRepository entrenamientoRepository)
        {
            _entrenamientoRepository = entrenamientoRepository;
        }

        public async Task<AsistenciaEntrenamientoResponseDto> Handle(RegistrarAsistenciaEntrenamientoCommand command)
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

            var inscripto = await _entrenamientoRepository.EstaInscriptoAsync(entrenamiento.Id, socio.Id);
            if (!inscripto)
            {
                throw new InvalidOperationException("El alumno no se encuentra inscripto en este entrenamiento.");
            }

            var fechaAsistencia = command.Fecha ?? DateTime.Today;

            var asistenciaExistente = await _entrenamientoRepository.ObtenerAsistenciaAsync(entrenamiento.Id, socio.Id, fechaAsistencia);

            Asistencia asistencia;
            if (asistenciaExistente != null)
            {
                asistenciaExistente.presente = command.Presente;
                asistencia = await _entrenamientoRepository.RegistrarAsistenciaAsync(asistenciaExistente);
            }
            else
            {
                var nuevaAsistencia = new Asistencia
                {
                    ActividadId = entrenamiento.Id,
                    SocioId = socio.Id,
                    Fecha = fechaAsistencia,
                    presente = command.Presente
                };
                asistencia = await _entrenamientoRepository.RegistrarAsistenciaAsync(nuevaAsistencia);
            }

            return new AsistenciaEntrenamientoResponseDto
            {
                Id = asistencia.Id,
                EntrenamientoId = entrenamiento.Id,
                SocioId = socio.Id,
                NombreSocio = $"{socio.Nombre} {socio.Apellido}".Trim(),
                Fecha = asistencia.Fecha,
                Presente = asistencia.presente
            };
        }
    }
}