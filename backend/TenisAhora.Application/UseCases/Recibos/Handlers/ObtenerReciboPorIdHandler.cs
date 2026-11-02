using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Recibos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Recibos.Queries;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Recibos.Handlers;

/// <summary>
/// Caso de Uso: Consulta de Recibo por ID [RF-70]
/// </summary>
public class ObtenerReciboPorIdHandler
{
    private readonly IReciboRepository _reciboRepository;

    public ObtenerReciboPorIdHandler(IReciboRepository reciboRepository)
    {
        _reciboRepository = reciboRepository;
    }

    public async Task<ReciboResponseDto> Handle(ObtenerReciboPorIdQuery query, CancellationToken ct = default)
    {
        var recibo = await _reciboRepository.ObtenerPorIdAsync(query.Id, ct);
        if (recibo is null)
        {
            throw new DomainException($"No se encontró el recibo con ID {query.Id}.");
        }

        return ReciboMapper.MapearADto(recibo);
    }
}
