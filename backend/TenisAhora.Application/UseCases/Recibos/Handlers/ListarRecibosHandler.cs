using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Recibos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Recibos.Queries;

namespace TenisAhora.Application.UseCases.Recibos.Handlers;

/// <summary>
/// Caso de Uso: Listar Todos los Recibos [RF-70]
/// </summary>
public class ListarRecibosHandler
{
    private readonly IReciboRepository _reciboRepository;

    public ListarRecibosHandler(IReciboRepository reciboRepository)
    {
        _reciboRepository = reciboRepository;
    }

    public async Task<List<ReciboResponseDto>> Handle(ListarRecibosQuery query, CancellationToken ct = default)
    {
        var recibos = await _reciboRepository.ListarTodosAsync(ct);
        return recibos.Select(ReciboMapper.MapearADto).ToList();
    }
}
