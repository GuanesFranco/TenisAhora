using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Entrenamientos.Queries;

namespace TenisAhora.Application.UseCases.Entrenamientos.Handlers
{
    public class GetEntrenamientoByIdHandler : IGetEntrenamientoByIdHandler
    {
        private readonly IEntrenamientoRepository _entrenamientoRepository;

        public GetEntrenamientoByIdHandler(IEntrenamientoRepository entrenamientoRepository)
        {
            _entrenamientoRepository = entrenamientoRepository;
        }

        public async Task<EntrenamientoResponseDto?> Handle(GetEntrenamientoByIdQuery query)
        {
            var e = await _entrenamientoRepository.ObtenerPorIdAsync(query.Id);
            if (e == null) return null;

            return new EntrenamientoResponseDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                ObjetivoEntrenamiento = e.objetivoEntrenamiento,
                HoraInicio = e.HoraInicio,
                HoraFin = e.HoraFin,
                CapacidadMaxma = e.CapacidadMaxma,
                CantidadDeAlumnos = e.CantidadDeAlumnos,
                Duracion = e.Duracion,
                EntrenadorId = e.EntrenadorId,
                NombreEntrenador = e.Entrenador != null ? $"{e.Entrenador.Nombre} {e.Entrenador.Apellido}".Trim() : null,
                Activo = e.Activo,
                NivelActividad = e.nivelActividad
            };
        }
    }
}