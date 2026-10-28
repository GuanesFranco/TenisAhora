using System;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Clases.Commands;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.UseCases.Clases.Handlers
{
    public class CreateClaseHandler : ICreateClaseHandler
    {
        private readonly IClaseRepository _claseRepository;

        public CreateClaseHandler(IClaseRepository claseRepository)
        {
            _claseRepository = claseRepository;
        }

        public async Task<ClaseResponseDto> Handle(CreateClaseCommand command)
        {
            // Regla de Negocio RD-02 / RF-42: El cupo no puede superar los 30 alumnos
            if (command.CapacidadMaxma > 30)
            {
                throw new InvalidOperationException("El cupo máximo de la clase no puede ser superior a 30 alumnos (RD-02).");
            }

            if (command.CapacidadMaxma <= 0)
            {
                throw new InvalidOperationException("El cupo de la clase debe ser mayor a 0.");
            }

            // Creamos la entidad del Dominio
            var clase = new Clase
            {
                Nombre = command.Nombre,
                TipoDeClase = command.TipoDeClase,
                HoraInicio = command.HoraInicio,
                HoraFin = command.HoraFin,
                CapacidadMaxma = command.CapacidadMaxma,
                CantidadDeAlumnos = 0,
                Duracion = command.Duracion,
                ProfesorId = command.ProfesorId,
                nivelActividad = command.NivelActividad,
                Activo = true
            };

            // La guardamos a través del repositorio
            var claseCreada = await _claseRepository.CrearAsync(clase);

            // Devolvemos el DTO con los datos guardados
            return new ClaseResponseDto
            {
                Id = claseCreada.Id,
                Nombre = claseCreada.Nombre,
                TipoDeClase = claseCreada.TipoDeClase,
                HoraInicio = claseCreada.HoraInicio,
                HoraFin = claseCreada.HoraFin,
                CapacidadMaxma = claseCreada.CapacidadMaxma,
                CantidadDeAlumnos = claseCreada.CantidadDeAlumnos,
                Duracion = claseCreada.Duracion,
                ProfesorId = claseCreada.ProfesorId,
                Activo = claseCreada.Activo,
                NivelActividad = claseCreada.nivelActividad
            };
        }
    }
}