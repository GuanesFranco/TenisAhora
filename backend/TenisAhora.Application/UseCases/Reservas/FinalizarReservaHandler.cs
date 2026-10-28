using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Reservas;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Reservas;

/// <summary>
/// Caso de Uso: Finalizar Reserva de Cancha y Devolución de Equipamiento
/// Concluye la reserva y reintegra el equipamiento asignado a la disponibilidad del inventario.
/// </summary>
public class FinalizarReservaHandler
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IStockReservasRepository _stockRepository;

    public FinalizarReservaHandler(
        IReservaRepository reservaRepository,
        IStockReservasRepository stockRepository)
    {
        _reservaRepository = reservaRepository;
        _stockRepository = stockRepository;
    }

    public async Task<ReservaResponseDto> Handle(int reservaId, CancellationToken cancellationToken = default)
    {
        // 1. Identificar la reserva mediante su ID
        var reserva = await _reservaRepository.ObtenerPorIdAsync(reservaId, cancellationToken);
        if (reserva is null)
        {
            throw new DomainException($"La reserva con ID {reservaId} no existe.");
        }

        // 2. Validaciones de estado
        if (reserva.estado == EstadoReserva.Finalizada)
        {
            throw new DomainException("La reserva ya se encuentra finalizada.");
        }

        if (reserva.estado == EstadoReserva.Cancelada)
        {
            throw new DomainException("No se puede finalizar una reserva que ha sido cancelada.");
        }

        if (reserva.estado != EstadoReserva.Confirmada)
        {
            throw new DomainException($"No se puede finalizar una reserva en estado '{reserva.estado}'. Debe estar 'Confirmada'.");
        }

        // 3. Validar que la reserva haya terminado su horario
        if (reserva.HoraFin > DateTime.Now)
        {
            throw new DomainException("No se puede finalizar una reserva antes de que termine su horario.");
        }

        // 4. Actualizar el estado de la reserva a Finalizada
        reserva.estado = EstadoReserva.Finalizada;
        await _reservaRepository.ActualizarReservaAsync(reserva, cancellationToken);

        // 5. Mapear respuesta
        var titularDni = reserva.Socios.FirstOrDefault()?.Dni ?? 0;
        var jugadoresDni = reserva.Socios.Select(s => s.Dni).ToList();

        var equipamientosResponse = reserva.DetallesStockReserva?.Select(d => new DetalleEquipamientoResponseDto(
            d.StockId,
            d.Stock?.tipo_elemento ?? string.Empty,
            d.cantidad
        )).ToList();

        return new ReservaResponseDto(
            reserva.Id,
            reserva.CanchaId,
            reserva.fecha_reserva,
            reserva.HoraInicio,
            reserva.HoraFin,
            titularDni,
            jugadoresDni,
            reserva.importe,
            reserva.importe * 0.5f,
            reserva.estado.ToString(),
            true,
            equipamientosResponse
        );
    }
}
