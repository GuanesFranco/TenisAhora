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
    public class AsignarProfesorParticularHandler : IAsignarProfesorParticularHandler
    {
        private readonly IClaseRepository _claseRepository;

        public AsignarProfesorParticularHandler(IClaseRepository claseRepository)
        {
            _claseRepository = claseRepository;
        }

        public async Task<ClaseParticularResponseDto> Handle(AsignarProfesorParticularCommand command)
        {
            // 1. Identificar al usuario mediante su DNI
            var socio = await _claseRepository.ObtenerSocioPorDniAsync(command.DniUsuario);
            if (socio == null)
            {
                throw new KeyNotFoundException($"No se encontró ningún socio registrado con el DNI {command.DniUsuario}.");
            }

            if (!socio.Activo)
            {
                throw new InvalidOperationException($"El socio con DNI {command.DniUsuario} se encuentra inactivo en el club.");
            }

            // 2. Seleccionar un profesor registrado y validar que esté activo
            var profesor = await _claseRepository.ObtenerProfesorPorIdAsync(command.ProfesorId);
            if (profesor == null || !profesor.Activo)
            {
                throw new KeyNotFoundException($"El profesor con ID {command.ProfesorId} no existe o no se encuentra activo.");
            }

            // Validar duración
            if (command.DuracionMinutos <= 0)
            {
                throw new InvalidOperationException("La duración de la clase particular debe ser mayor a 0 minutos.");
            }

            var horaInicio = command.FechaHoraInicio;
            var horaFin = horaInicio.AddMinutes(command.DuracionMinutos);

            // 3. Crear la clase particular asignando el profesor y capacidad para 1 alumno
            var claseParticular = new Clase
            {
                Nombre = $"Clase Particular - {socio.Nombre} {socio.Apellido}",
                TipoDeClase = command.TipoDeClase,
                HoraInicio = horaInicio,
                HoraFin = horaFin,
                Duracion = command.DuracionMinutos,
                CapacidadMaxma = 1,
                CantidadDeAlumnos = 1,
                ProfesorId = profesor.Id,
                nivelActividad = command.Nivel,
                Activo = true
            };

            // 4. Registrar la inscripción del socio a la clase particular
            var inscripcion = new Inscripcion_actividad
            {
                SocioId = socio.Id,
                Fecha = DateTime.UtcNow,
                estadoInscripcion = EstadoInscripcion.Confirmada,
                NivelActividad = command.Nivel,
                Actividad = claseParticular
            };

            var claseGuardada = await _claseRepository.RegistrarClaseParticularAsync(claseParticular, inscripcion);

            // 5. Devolver el DTO con la confirmación de la asignación
            return new ClaseParticularResponseDto
            {
                ClaseId = claseGuardada.Id,
                NombreClase = claseGuardada.Nombre,
                ProfesorId = profesor.Id,
                NombreProfesor = $"{profesor.Nombre} {profesor.Apellido}",
                SocioId = socio.Id,
                NombreSocio = $"{socio.Nombre} {socio.Apellido}",
                DniSocio = socio.Dni,
                HoraInicio = claseGuardada.HoraInicio,
                HoraFin = claseGuardada.HoraFin,
                Duracion = claseGuardada.Duracion,
                Mensaje = $"Profesor {profesor.Nombre} {profesor.Apellido} asignado exitosamente al socio {socio.Nombre} {socio.Apellido} para clase particular."
            };
        }
    }
}