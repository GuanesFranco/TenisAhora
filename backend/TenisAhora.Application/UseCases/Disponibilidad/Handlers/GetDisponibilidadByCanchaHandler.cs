using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Disponibilidad;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Disponibilidad.Queries;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.UseCases.Disponibilidad.Handlers;

public class GetDisponibilidadByCanchaHandler : IGetDisponibilidadByCanchaHandler
{
    private readonly IDisponibilidadRepository _repository;

    public GetDisponibilidadByCanchaHandler(IDisponibilidadRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<DisponibilidadResponseDto>> Handle(GetDisponibilidadByCanchaQuery query, CancellationToken cancellationToken = default)
    {
        var disponibilidades = await _repository.ObtenerPorCanchaYFiltrosAsync(
            query.CanchaId, query.FechaDesde, query.FechaHasta, query.Estado, cancellationToken);

        return disponibilidades.Select(d => new DisponibilidadResponseDto(
            d.Id,
            d.CanchaId,
            d.Cancha?.TipoDeCancha?.superficie.ToString() ?? string.Empty,
            d.FechaHoraInicio,
            d.FechaHoraFin,
            d.estado,
            d.estado.ToString()
        ));
    }
}