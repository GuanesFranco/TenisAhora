using System;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Clases.Commands;

namespace TenisAhora.Application.UseCases.Clases.Handlers
{
    public class UpdateClaseHandler : IUpdateClaseHandler
    {
        private readonly IClaseRepository _claseRepository;

        public UpdateClaseHandler(IClaseRepository claseRepository)
        {
            _claseRepository = claseRepository;
        }

        public async Task<ClaseResponseDto?> Handle(int id, UpdateClaseCommand command)
        {
            var clase = await _claseRepository.ObtenerPorIdAsync(id);
            if (clase == null) return null;

            // Regla de Negocio RD-02: El cupo no puede superar los 30 alumnos
            if (command.CapacidadMaxma > 30)
            {
                throw new InvalidOperationException("El cupo máximo de la clase no puede ser superior a 30 alumnos (RD-02).");
            }

            if (command.CapacidadMaxma <= 0)
            {
                throw new InvalidOperationException("El cupo de la clase debe ser mayor a 0.");
            }

            // Actualizamos los datos
            clase.Nombre = command.Nombre;
            clase.TipoDeClase = command.TipoDeClase;
            clase.HoraInicio = command.HoraInicio;
            clase.HoraFin = command.HoraFin;
            clase.CapacidadMaxma = command.CapacidadMaxma;
            clase.Duracion = command.Duracion;
            clase.ProfesorId = command.ProfesorId;
            clase.nivelActividad = command.NivelActividad;

            await _claseRepository.ModificarAsync(clase);

            return new ClaseResponseDto
            {
                Id = clase.Id,
                Nombre = clase.Nombre,
                TipoDeClase = clase.TipoDeClase,
                HoraInicio = clase.HoraInicio,
                HoraFin = clase.HoraFin,
                CapacidadMaxma = clase.CapacidadMaxma,
                CantidadDeAlumnos = clase.CantidadDeAlumnos,
                Duracion = clase.Duracion,
                ProfesorId = clase.ProfesorId,
                NombreProfesor = clase.Profesor != null ? $"{clase.Profesor.Nombre} {clase.Profesor.Apellido}" : null,
                Activo = clase.Activo,
                NivelActividad = clase.nivelActividad
            };
        }
    }
}