using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Entrenadores.Commands;

namespace TenisAhora.Application.UseCases.Entrenadores.Handlers
{
    public class UpdateEntrenadorHandler : IUpdateEntrenadorHandler
    {
        private readonly IEntrenadorRepository _entrenadorRepository;

        public UpdateEntrenadorHandler(
            IEntrenadorRepository entrenadorRepository)
        {
            _entrenadorRepository = entrenadorRepository;
        }

        public async Task<EntrenadorResponseDto?> Handle(
            int id,
            UpdateEntrenadorCommand command)
        {
            var entrenador =
                await _entrenadorRepository
                    .ObtenerPorIdParaActualizarAsync(id);

            if (entrenador is null)
            {
                return null;
            }

            // Actualizar datos personales.
            entrenador.Nombre = command.Nombre;
            entrenador.Apellido = command.Apellido;
            entrenador.Dni = command.Dni;
            entrenador.FechaDeNacimiento = command.FechaDeNacimiento;
            entrenador.Telefono = command.Telefono;
            entrenador.Direccion = command.Direccion;
            entrenador.CodigoPostal = command.CodigoPostal;
            entrenador.Email = command.Email;

            // Actualizar datos del instructor.
            entrenador.especialidad = command.Especialidad;
            entrenador.antiguedad = command.Antiguedad;

            // Actualizar datos específicos del entrenador.
            entrenador.Objetivo = command.Objetivo;

            await _entrenadorRepository.GuardarCambiosAsync();

            var certificacion = entrenador.certificacionDeportiva;

            return new EntrenadorResponseDto
            {
                Id = entrenador.Id,

                Nombre = entrenador.Nombre,
                Apellido = entrenador.Apellido,
                Dni = entrenador.Dni,
                FechaDeNacimiento = entrenador.FechaDeNacimiento,
                Telefono = entrenador.Telefono,
                Direccion = entrenador.Direccion,
                CodigoPostal = entrenador.CodigoPostal,
                Email = entrenador.Email,

                Especialidad = entrenador.especialidad,
                Antiguedad = entrenador.antiguedad,
                AdministradorId = entrenador.AdministradorId,

                Objetivo = entrenador.Objetivo,
                Activo = entrenador.Activo,

                CertificacionId = certificacion?.Id,
                NombreCertificacion = certificacion?.NombreCertificacion,
                EnteEmisor = certificacion?.EnteEmisor,
                FechaEmision = certificacion?.FechaEmision,
                CertificacionVerificada =
                    certificacion?.certificacionDeportiva
            };
        }


    }
}
