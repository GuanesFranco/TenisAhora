using System;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Stock.Commands;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Stock.Handlers;

/// <summary>
/// Caso de Uso: Finalizar Alquiler de Stock de Reserva y Restituir Cantidades [RF-21, RD-16]
/// </summary>
public class FinalizarAlquilerStockHandler
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IStockReservasRepository _stockRepository;

    public FinalizarAlquilerStockHandler(
        IReservaRepository reservaRepository,
        IStockReservasRepository stockRepository)
    {
        _reservaRepository = reservaRepository;
        _stockRepository = stockRepository;
    }

    public async Task Handle(FinalizarAlquilerStockCommand command, CancellationToken cancellationToken = default)
    {
        var reserva = await _reservaRepository.ObtenerPorIdAsync(command.ReservaId, cancellationToken);
        if (reserva is null)
        {
            throw new DomainException("Reserva no encontrada.");
        }

        if (reserva.estado != EstadoReserva.Confirmada)
        {
            throw new DomainException("Solo se puede finalizar y restituir stock de una reserva en estado Confirmada.");
        }

        foreach (var detalle in reserva.DetallesStockReserva)
        {
            await _stockRepository.IncrementarStockAsync(detalle.StockId, detalle.cantidad, cancellationToken);
        }
    }
}
