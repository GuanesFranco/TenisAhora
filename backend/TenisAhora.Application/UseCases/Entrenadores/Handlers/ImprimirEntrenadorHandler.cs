using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Entrenadores.Handlers
{
    public class ImprimirEntrenadorHandler : IImprimirEntrenadorHandler
    {

        private readonly IEntrenadorRepository _entrenadorRepository;

        public ImprimirEntrenadorHandler(
            IEntrenadorRepository entrenadorRepository)
        {
            _entrenadorRepository = entrenadorRepository;
        }

        public async Task<EntrenadorResponseDto?> Handle(int id)
        {
            var entrenador =
                await _entrenadorRepository.ObtenerParaImprimirAsync(id);

            if (entrenador == null)
                return null;

            var certificacion = entrenador.certificacionDeportiva;

            return new EntrenadorResponseDto
            {
                Id = entrenador.Id,

                // Datos personales
                Nombre = entrenador.Nombre,
                Apellido = entrenador.Apellido,
                Dni = entrenador.Dni,
                FechaDeNacimiento = entrenador.FechaDeNacimiento,
                Telefono = entrenador.Telefono,
                Direccion = entrenador.Direccion,
                CodigoPostal = entrenador.CodigoPostal,
                Email = entrenador.Email,

                // Datos de instructor
                Especialidad = entrenador.especialidad,
                Antiguedad = entrenador.antiguedad,
                AdministradorId = entrenador.AdministradorId,

                // Datos de entrenador
                Objetivo = entrenador.Objetivo,
                Activo = entrenador.Activo,

                // Certificación deportiva
                CertificacionId = certificacion?.Id,
                NombreCertificacion = certificacion?.NombreCertificacion,
                EnteEmisor = certificacion?.EnteEmisor,
                FechaEmision = certificacion?.FechaEmision,

                CertificacionVerificada = certificacion == null
                    ? null
                    : certificacion.EstadoCertificacion
                        == EstadoCertificacion.Aprobada
            };
        }


    }
}
