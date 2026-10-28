using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Clases.Queries;

namespace TenisAhora.Application.UseCases.Clases.Handlers
{
    public class GetClasesHandler : IGetClasesHandler
    {
        private readonly IClaseRepository _claseRepository;

        public GetClasesHandler(IClaseRepository claseRepository)
        {
            _claseRepository = claseRepository;
        }

        public async Task<IEnumerable<ClaseResponseDto>> Handle(GetClasesQuery query)
        {
            var clases = await _claseRepository.ObtenerTodasAsync();

            return clases.Select(clase => new ClaseResponseDto
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
            });
        }
    }
}