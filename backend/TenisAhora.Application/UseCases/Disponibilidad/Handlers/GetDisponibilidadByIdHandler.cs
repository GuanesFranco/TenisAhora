using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Disponibilidad;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Disponibilidad.Queries;

namespace TenisAhora.Application.UseCases.Disponibilidad.Handlers;

public class GetDisponibilidadByIdHandler : IGetDisponibilidadByIdHandler
{
    private readonly IDisponibilidadRepository _repository;

    public GetDisponibilidadByIdHandler(IDisponibilidadRepository repository)
    {
        _repository = repository;
    }

    public async Task<DisponibilidadResponseDto?> Handle(GetDisponibilidadByIdQuery query, CancellationToken cancellationToken = default)
    {
        var d = await _repository.ObtenerPorIdAsync(query.Id, cancellationToken);
        
        if (d is null)
        {
            return null;
        }

        return new DisponibilidadResponseDto(
            d.Id,
            d.CanchaId,
            d.Cancha?.TipoDeCancha?.superficie.ToString() ?? string.Empty,
            d.FechaHoraInicio,
            d.FechaHoraFin,
            d.estado,
            d.estado.ToString()
        );
    }
}