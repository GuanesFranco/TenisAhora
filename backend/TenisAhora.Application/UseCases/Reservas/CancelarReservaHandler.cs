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
/// Caso de Uso: Cancelar Reserva de Cancha
/// [RF - 27] El sistema debe permitir cancelar reservas de canchas.
/// ● El sistema debe permitir identificar la reserva mediante su ID.
/// ● El sistema debe permitir cancelar la reserva seleccionada.
/// ● El sistema debe aplicar un cargo al usuario si la cancelación se realiza fuera del plazo mínimo de antelación.
/// ● El sistema debe procesar un reembolso si la cancelación se realiza dentro del plazo de antelación establecido.
/// ● El sistema debe permitir al personal autorizado configurar el plazo mínimo de antelación requerida para las cancelaciones.
/// </summary>
public class CancelarReservaHandler
{
    private readonly IReservaRepository _reservaRepository;
    private readonly IStockRepository _stockRepository;

    /// <summary>
    /// [RF - 27] Plazo mínimo de antelación en horas configurable por personal autorizado (por defecto 6 horas).
    /// </summary>
    public static double HorasMinimasAntelacion { get; set; } = 6.0;

    public CancelarReservaHandler(
        IReservaRepository reservaRepository,
        IStockRepository stockRepository)
    {
        _reservaRepository = reservaRepository;
        _stockRepository = stockRepository;
    }

    public async Task<CancelarReservaResponseDto> Handle(int reservaId, CancellationToken cancellationToken = default)
    {
        // 1. [RF - 27] Identificar la reserva mediante su ID
        var reserva = await _reservaRepository.ObtenerPorIdAsync(reservaId, cancellationToken);
        if (reserva is null)
        {
            throw new DomainException($"La reserva con ID {reservaId} no existe.");
        }

        // 2. Validar que no se encuentre cancelada previamente
        if (reserva.estado == EstadoReserva.Cancelada)
        {
            throw new DomainException("La reserva ya se encuentra cancelada.");
        }

        // 3. Validar que no haya finalizado el horario
        if (reserva.HoraInicio <= DateTime.Now)
        {
            throw new DomainException("No se puede cancelar una reserva que ya ha comenzado o finalizado.");
        }

        // 4. [RF - 27] Plazo mínimo de antelación configurable por personal autorizado
        double horasRestantes = (reserva.HoraInicio - DateTime.Now).TotalHours;

        // [RF - 27] Procesar reembolso si es dentro del plazo, o aplicar retención/cargo si es fuera del plazo
        bool aplicaReembolso = horasRestantes >= HorasMinimasAntelacion;
        string mensajeResultado = aplicaReembolso
            ? $"Reserva cancelada dentro del plazo establecido ({HorasMinimasAntelacion} hs de antelación). Se procesará el reembolso correspondiente."
            : $"Reserva cancelada fuera de término (menos de {HorasMinimasAntelacion} hs de anticipación). Se retendrá la seña abonada en concepto de cargo por cancelación tardía.";

        // 5. Reintegrar el equipamiento al stock general (restitución física al inventario)
        if (reserva.DetallesStockReserva != null && reserva.DetallesStockReserva.Any())
        {
            foreach (var detalle in reserva.DetallesStockReserva)
            {
                await _stockRepository.IncrementarStockAsync(detalle.StockId, detalle.cantidad, cancellationToken);
            }
        }

        // 6. [RF - 27] Cancelar la reserva seleccionada
        reserva.estado = EstadoReserva.Cancelada;
        await _reservaRepository.ActualizarReservaAsync(reserva, cancellationToken);

        return new CancelarReservaResponseDto(
            reserva.Id,
            mensajeResultado,
            aplicaReembolso,
            reserva.estado.ToString()
        );
    }
}