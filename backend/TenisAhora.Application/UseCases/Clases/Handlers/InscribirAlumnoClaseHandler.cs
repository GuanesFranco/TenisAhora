using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Clases.Commands;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Clases.Handlers
{
    public class InscribirAlumnoClaseHandler : IInscribirAlumnoClaseHandler
    {
        private readonly IClaseRepository _claseRepository;

        public InscribirAlumnoClaseHandler(IClaseRepository claseRepository)
        {
            _claseRepository = claseRepository;
        }

        public async Task<InscripcionClaseResponseDto> Handle(InscribirAlumnoClaseCommand command)
        {
            // 1. Identificar la clase mediante su ID y verificar que exista y esté activa
            var clase = await _claseRepository.ObtenerPorIdAsync(command.ClaseId);
            if (clase == null || !clase.Activo)
            {
                throw new KeyNotFoundException($"No se encontró la clase con ID {command.ClaseId} o se encuentra inactiva.");
            }

            // 2. Identificar al usuario mediante su DNI
            var socio = await _claseRepository.ObtenerSocioPorDniAsync(command.DniUsuario);
            if (socio == null)
            {
                throw new KeyNotFoundException($"No se encontró ningún socio registrado con el DNI {command.DniUsuario}.");
            }

            if (!socio.Activo)
            {
                throw new InvalidOperationException($"El socio con DNI {command.DniUsuario} se encuentra inactivo en el club.");
            }

            // 3. Regla RF-46 / RD-02: No permitir la inscripción si se alcanzó el cupo límite
            if (clase.CantidadDeAlumnos >= clase.CapacidadMaxma || clase.CantidadDeAlumnos >= 30)
            {
                throw new InvalidOperationException("No se puede realizar la inscripción: se alcanzó el cupo límite de la clase.");
            }

            // 4. Validar que el socio no esté ya inscripto en esta clase
            var yaInscripto = await _claseRepository.EstaInscriptoAsync(clase.Id, socio.Id);
            if (yaInscripto)
            {
                throw new InvalidOperationException($"El socio con DNI {command.DniUsuario} ya se encuentra inscripto en esta clase.");
            }

            // 5. Registrar la inscripción
            var nuevaInscripcion = new Inscripcion_actividad
            {
                ActividadId = clase.Id,
                SocioId = socio.Id,
                Fecha = DateTime.UtcNow,
                estadoInscripcion = EstadoInscripcion.Confirmada,
                NivelActividad = clase.nivelActividad
            };

            // Sumamos un alumno a la cantidad actual de la clase
            clase.CantidadDeAlumnos += 1;

            var inscripcionGuardada = await _claseRepository.InscribirAlumnoAsync(nuevaInscripcion, clase);

            // 6. Devolver el DTO con los datos confirmados
            return new InscripcionClaseResponseDto
            {
                InscripcionId = inscripcionGuardada.Id,
                ClaseId = clase.Id,
                NombreClase = clase.Nombre,
                SocioId = socio.Id,
                NombreSocio = socio.Nombre,
                ApellidoSocio = socio.Apellido,
                DniSocio = socio.Dni,
                FechaInscripcion = inscripcionGuardada.Fecha,
                Estado = inscripcionGuardada.estadoInscripcion.ToString(),
                CantidadDeAlumnosActual = clase.CantidadDeAlumnos,
                CapacidadMaxima = clase.CapacidadMaxma
            };
        }
    }
}