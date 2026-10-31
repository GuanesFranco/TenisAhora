using System;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Stock;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Stock.Queries;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Stock.Handlers;

/// <summary>
/// Caso de Uso: Obtener Stock por ID [RF-20]
/// </summary>
public class ObtenerStockPorIdHandler
{
    private readonly IStockRepository _stockRepository;

    public ObtenerStockPorIdHandler(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<StockResponseDto> Handle(ObtenerStockPorIdQuery query, CancellationToken cancellationToken = default)
    {
        var stock = await _stockRepository.ObtenerPorIdAsync(query.Id, cancellationToken);
        if (stock is null)
        {
            throw new DomainException($"No se encontró equipamiento con ID {query.Id}.");
        }

        return new StockResponseDto(stock.Id, stock.tipo_elemento, stock.cantidad_disponible);
    }
}
