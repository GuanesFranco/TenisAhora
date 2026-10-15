using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Profesores.Handlers
{
    public class ImprimirProfesorHandler : IImprimirProfesorHandler
    {
        private readonly IProfesorRepository _profesorRepository;

        public ImprimirProfesorHandler(
            IProfesorRepository profesorRepository)
        {
            _profesorRepository = profesorRepository;
        }

        public async Task<ProfesorResponseDto?> Handle(int id)
        {
            var profesor =
                await _profesorRepository.ObtenerParaImprimirAsync(id);

            if (profesor == null)
                return null;

            var certificacion = profesor.certificacionDeportiva;

            return new ProfesorResponseDto
            {
                Id = profesor.Id,

                // Datos personales
                Nombre = profesor.Nombre,
                Apellido = profesor.Apellido,
                Dni = profesor.Dni,
                FechaDeNacimiento = profesor.FechaDeNacimiento,
                Telefono = profesor.Telefono,
                Direccion = profesor.Direccion,
                CodigoPostal = profesor.CodigoPostal,
                Email = profesor.Email,

                // Datos de instructor
                Especialidad = profesor.especialidad,
                Antiguedad = profesor.antiguedad,
                AdministradorId = profesor.AdministradorId,

                // Datos de profesor
                TipoDeEnsenanza = profesor.TipoDeEnsenanza,
                Activo = profesor.Activo,

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
