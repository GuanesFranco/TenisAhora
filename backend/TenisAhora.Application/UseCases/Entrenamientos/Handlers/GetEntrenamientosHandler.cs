using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Entrenamientos.Queries;

namespace TenisAhora.Application.UseCases.Entrenamientos.Handlers
{
    public class GetEntrenamientosHandler : IGetEntrenamientosHandler
    {
        private readonly IEntrenamientoRepository _entrenamientoRepository;

        public GetEntrenamientosHandler(IEntrenamientoRepository entrenamientoRepository)
        {
            _entrenamientoRepository = entrenamientoRepository;
        }

        public async Task<IEnumerable<EntrenamientoResponseDto>> Handle(GetEntrenamientosQuery query)
        {
            var lista = await _entrenamientoRepository.ObtenerTodosAsync();

            return lista.Select(e => new EntrenamientoResponseDto
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
            });
        }
    }
}