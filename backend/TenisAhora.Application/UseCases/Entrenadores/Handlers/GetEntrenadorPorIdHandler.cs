using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Entrenadores.Handlers
{
    public class GetEntrenadorPorIdHandler : IGetEntrenadorPorIdHandler
    {
        private readonly IEntrenadorRepository _entrenadorRepository;

        public GetEntrenadorPorIdHandler(IEntrenadorRepository entrenadorRepository)
        {
            _entrenadorRepository = entrenadorRepository;
        }


        public async Task<EntrenadorResponseDto?> Handle(int id)
        {
            var entrenador = await _entrenadorRepository.ObtenerPorIdAsync(id);

            if (entrenador == null)
                return null;

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
                CertificacionVerificada = certificacion == null
    ? null
    : certificacion.EstadoCertificacion == EstadoCertificacion.Aprobada

            };
        }

    }

}

