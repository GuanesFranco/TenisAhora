using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Reservas;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Reservas;

/// <summary>
/// Caso de Uso: Confirmar Reserva mediante Validación del Pago
/// [RF - 24] El sistema no debe confirmar la reserva si no se encuentra registrado y validado el pago del 50% del alquiler.
/// </summary>
public class ConfirmarReservaHandler
{
    private readonly IReservaRepository _reservaRepository;

    public ConfirmarReservaHandler(IReservaRepository reservaRepository)
    {
        _reservaRepository = reservaRepository;
    }

    /// <summary>
    /// Valida que la reserva tenga registrado en el sistema el pago del 50% de la seña y la pasa a estado Confirmada.
    /// </summary>
    public async Task<ReservaResponseDto> Handle(int reservaId, CancellationToken cancellationToken = default)
    {
        // 1. Obtener la reserva
        var reserva = await _reservaRepository.ObtenerPorIdAsync(reservaId, cancellationToken);
        if (reserva is null)
        {
            throw new DomainException($"No se encontró la reserva con ID {reservaId}.");
        }

        // 2. Validar que la reserva no esté cancelada ni ya confirmada
        if (reserva.estado == EstadoReserva.Cancelada)
        {
            throw new DomainException("No se puede confirmar una reserva que ha sido cancelada.");
        }

        if (reserva.estado == EstadoReserva.Confirmada)
        {
            throw new DomainException("La reserva ya se encuentra confirmada.");
        }

        // 3. [RF - 24] Validar que la reserva tenga registrado en BD el pago del 50% hecho por Cobros
        var totalPagado = reserva.Pagos
            .Where(p => p.estado == EstadoPago.Validado)
            .Sum(p => p.importe);

        float montoMinimoSena = reserva.importe * 0.5f;

        if (totalPagado < montoMinimoSena)
        {
            throw new DomainException(
                $"No se puede confirmar: No se encuentra registrado y validado el pago del 50% de la seña en el sistema de cobros. Total pagado: ${totalPagado:F2}, Seña requerida: ${montoMinimoSena:F2}.");
        }

        // 4. Si ya está pagado el 50%, confirmar la reserva
        reserva.estado = EstadoReserva.Confirmada;
        await _reservaRepository.ActualizarReservaAsync(reserva, cancellationToken);

        return MapToDto(reserva);
    }

    private static ReservaResponseDto MapToDto(Reserva reserva)
    {
        var titularDni = reserva.Socios.FirstOrDefault()?.Dni ?? 0;
        var jugadoresDni = reserva.Socios.Select(s => s.Dni).ToList();

        var equipamientosResponse = reserva.DetallesStockReserva.Select(d => new DetalleEquipamientoResponseDto(
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
