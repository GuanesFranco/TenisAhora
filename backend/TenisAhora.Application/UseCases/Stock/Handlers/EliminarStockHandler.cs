using System;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Stock.Commands;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Stock.Handlers;

/// <summary>
/// Caso de Uso: Eliminar Equipamiento del Stock [RF-23]
/// </summary>
public class EliminarStockHandler
{
    private readonly IStockRepository _stockRepository;

    public EliminarStockHandler(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task Handle(EliminarStockCommand command, CancellationToken cancellationToken = default)
    {
        var stock = await _stockRepository.ObtenerPorIdAsync(command.Id, cancellationToken);
        if (stock is null)
        {
            throw new DomainException($"No se encontró equipamiento con ID {command.Id}.");
        }

        var tieneAsociadas = await _stockRepository.TieneReservasAsociadasAsync(command.Id, cancellationToken);
        if (tieneAsociadas)
        {
            throw new DomainException("No se puede eliminar el equipamiento porque se encuentra asociado a reservas registradas.");
        }

        await _stockRepository.EliminarAsync(stock, cancellationToken);
    }
}
