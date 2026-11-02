using System;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Stock;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Stock.Commands;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Stock.Handlers;

/// <summary>
/// Caso de Uso: Registrar Stock de Equipamiento Deportivo [RF-19, RD-14]
/// </summary>
public class RegistrarStockHandler
{
    private readonly IStockReservasRepository _stockRepository;

    public RegistrarStockHandler(IStockReservasRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<StockResponseDto> Handle(RegistrarStockCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.TipoElemento))
        {
            throw new DomainException("El tipo de elemento es obligatorio.");
        }

        if (command.CantidadDisponible < 0)
        {
            throw new DomainException("La cantidad disponible no puede ser negativa.");
        }

        var stock = new TenisAhora.Domain.Entities.Stock
        {
            tipo_elemento = command.TipoElemento.Trim(),
            cantidad_disponible = command.CantidadDisponible
        };

        var creado = await _stockRepository.AgregarAsync(stock, cancellationToken);
        return new StockResponseDto(creado.Id, creado.tipo_elemento, creado.cantidad_disponible);
    }
}
