using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Clases.Commands;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.UseCases.Clases.Handlers
{
    public class RegistrarAsistenciaClaseHandler : IRegistrarAsistenciaClaseHandler
    {
        private readonly IClaseRepository _claseRepository;

        public RegistrarAsistenciaClaseHandler(IClaseRepository claseRepository)
        {
            _claseRepository = claseRepository;
        }

        public async Task<AsistenciaClaseResponseDto> Handle(RegistrarAsistenciaClaseCommand command)
        {
            // 1. Identificar la clase mediante su ID
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

            // 3. Validar que el usuario esté inscripto en esta clase
            var estaInscripto = await _claseRepository.EstaInscriptoAsync(clase.Id, socio.Id);
            if (!estaInscripto)
            {
                throw new InvalidOperationException($"El socio con DNI {command.DniUsuario} no se encuentra inscripto en esta clase.");
            }

            // 4. Determinar la fecha (la indicada o la actual)
            var fechaAsistencia = command.Fecha ?? DateTime.UtcNow;

            // 5. Verificar si ya existe registro de asistencia para ese día
            var asistenciaExistente = await _claseRepository.ObtenerAsistenciaAsync(clase.Id, socio.Id, fechaAsistencia);

            Asistencia asistenciaGuardada;
            string mensaje;

            if (asistenciaExistente != null)
            {
                // Actualizamos el estado (presente/ausente)
                asistenciaExistente.presente = command.Presente;
                asistenciaGuardada = await _claseRepository.RegistrarAsistenciaAsync(asistenciaExistente);
                mensaje = command.Presente 
                    ? "Asistencia actualizada correctamente: Presente." 
                    : "Asistencia actualizada correctamente: Ausente.";
            }
            else
            {
                // Creamos un nuevo registro de asistencia
                var nuevaAsistencia = new Asistencia
                {
                    ActividadId = clase.Id,
                    SocioId = socio.Id,
                    Fecha = fechaAsistencia,
                    presente = command.Presente
                };

                asistenciaGuardada = await _claseRepository.RegistrarAsistenciaAsync(nuevaAsistencia);
                mensaje = command.Presente 
                    ? "Asistencia registrada correctamente: Presente." 
                    : "Asistencia registrada correctamente: Ausente.";
            }

            // 6. Devolver el DTO de respuesta
            return new AsistenciaClaseResponseDto
            {
                AsistenciaId = asistenciaGuardada.Id,
                ClaseId = clase.Id,
                NombreClase = clase.Nombre,
                SocioId = socio.Id,
                NombreSocio = socio.Nombre,
                ApellidoSocio = socio.Apellido,
                DniSocio = socio.Dni,
                Fecha = asistenciaGuardada.Fecha,
                Presente = asistenciaGuardada.presente,
                Mensaje = mensaje
            };
        }
    }
}