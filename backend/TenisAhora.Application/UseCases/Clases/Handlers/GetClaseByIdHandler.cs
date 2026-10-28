using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Clases.Queries;

namespace TenisAhora.Application.UseCases.Clases.Handlers
{
    public class GetClaseByIdHandler : IGetClaseByIdHandler
    {
        private readonly IClaseRepository _claseRepository;

        public GetClaseByIdHandler(IClaseRepository claseRepository)
        {
            _claseRepository = claseRepository;
        }

        public async Task<ClaseResponseDto?> Handle(GetClaseByIdQuery query)
        {
            var clase = await _claseRepository.ObtenerPorIdAsync(query.Id);
            if (clase == null) return null;

            return new ClaseResponseDto
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
            };
        }
    }
}