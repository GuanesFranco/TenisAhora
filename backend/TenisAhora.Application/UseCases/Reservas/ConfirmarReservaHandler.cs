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

    public async Task<ReservaResponseDto> Handle(int reservaId, RegistrarPagoSenaCommand command, CancellationToken cancellationToken = default)
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

        // 3. [RF - 24] Validar que el pago cubra como mínimo el 50% del alquiler total
        float montoMinimoSena = reserva.importe * 0.5f;
        if (command.ImportePago < montoMinimoSena)
        {
            throw new DomainException(
                $"El pago registrado (${command.ImportePago:F2}) es insuficiente para confirmar la reserva. Se requiere como mínimo el 50% del total (${montoMinimoSena:F2}).");
        }

        // 4. [RF - 24] Registrar y validar el pago en la entidad
        var pago = new Pago
        {
            ReservaId = reserva.Id,
            importe = command.ImportePago,
            Fecha_pago = DateTime.Now,
            tipo_pago = command.TipoPago,
            estado = EstadoPago.Validado,
            porcentaje_pago = (command.ImportePago / reserva.importe) * 100f
        };

        reserva.Pagos.Add(pago);

        // 5. [RF - 24] Cambiar el estado a Confirmada tras validar el pago
        reserva.estado = EstadoReserva.Confirmada;

        await _reservaRepository.ActualizarReservaAsync(reserva, cancellationToken);

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
            montoMinimoSena,
            reserva.estado.ToString(),
            true,
            equipamientosResponse
        );
    }
}
