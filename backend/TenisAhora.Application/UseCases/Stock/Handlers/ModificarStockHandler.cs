using System;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Stock;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Stock.Commands;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Stock.Handlers;

/// <summary>
/// Caso de Uso: Modificar Cantidad de Stock Disponible [RF-22, RD-15]
/// </summary>
public class ModificarStockHandler
{
    private readonly IStockRepository _stockRepository;

    public ModificarStockHandler(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<StockResponseDto> Handle(ModificarStockCommand command, CancellationToken cancellationToken = default)
    {
        if (command.CantidadDisponible < 0)
        {
            throw new DomainException("La cantidad disponible no puede ser negativa.");
        }

        var stock = await _stockRepository.ObtenerPorIdAsync(command.Id, cancellationToken);
        if (stock is null)
        {
            throw new DomainException($"No se encontró equipamiento con ID {command.Id}.");
        }

        stock.cantidad_disponible = command.CantidadDisponible;
        await _stockRepository.ActualizarAsync(stock, cancellationToken);

        return new StockResponseDto(stock.Id, stock.tipo_elemento, stock.cantidad_disponible);
    }
}
