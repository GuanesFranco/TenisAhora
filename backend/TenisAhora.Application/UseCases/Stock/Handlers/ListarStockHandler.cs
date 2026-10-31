using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Stock;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Stock.Queries;

namespace TenisAhora.Application.UseCases.Stock.Handlers;

/// <summary>
/// Caso de Uso: Listar Todo el Stock de Equipamiento Deportivo [RF-20]
/// </summary>
public class ListarStockHandler
{
    private readonly IStockRepository _stockRepository;

    public ListarStockHandler(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<List<StockResponseDto>> Handle(ListarStockQuery query, CancellationToken cancellationToken = default)
    {
        var items = await _stockRepository.ListarTodosAsync(cancellationToken);
        return items.Select(s => new StockResponseDto(s.Id, s.tipo_elemento, s.cantidad_disponible)).ToList();
    }
}
