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
    public class GetProfesoresHandler : IGetProfesoresHandler
    {
        private readonly IProfesorRepository _profesorRepository;

        public GetProfesoresHandler(
            IProfesorRepository profesorRepository)
        {
            _profesorRepository = profesorRepository;
        }

        public async Task<List<ProfesorResponseDto>> Handle(
            GetProfesoresQuery query)
        {
            var profesores =
                await _profesorRepository.ObtenerTodosAsync();

            return profesores.Select(profesor =>
            {
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
                    EstadoCertificacion = certificacion?.EstadoCertificacion,
                    CertificacionVerificada =
                        certificacion?.certificacionDeportiva
                };
            }).ToList();
        }
    }
}
