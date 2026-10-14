using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Profesores.Queries;

namespace TenisAhora.Application.UseCases.Profesores.Handlers
{
    public class GetProfesorByIdHandler : IGetProfesorByIdHandler
    {
        private readonly IProfesorRepository _profesorRepository;

        public GetProfesorByIdHandler(
            IProfesorRepository profesorRepository)
        {
            _profesorRepository = profesorRepository;
        }

        public async Task<ProfesorResponseDto?> Handle(
            GetProfesorByIdQuery query)
        {
            var profesor =
                await _profesorRepository.ObtenerPorIdAsync(query.Id);

            if (profesor is null)
            {
                return null;
            }

            var certificacion = profesor.certificacionDeportiva;

            return new ProfesorResponseDto
            {
                Id = profesor.Id,

                Nombre = profesor.Nombre,
                Apellido = profesor.Apellido,
                Dni = profesor.Dni,
                FechaDeNacimiento = profesor.FechaDeNacimiento,
                Telefono = profesor.Telefono,
                Direccion = profesor.Direccion,
                CodigoPostal = profesor.CodigoPostal,
                Email = profesor.Email,

                Especialidad = profesor.especialidad,
                Antiguedad = profesor.antiguedad,
                AdministradorId = profesor.AdministradorId,

                TipoDeEnsenanza = profesor.TipoDeEnsenanza,
                Activo = profesor.Activo,

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
