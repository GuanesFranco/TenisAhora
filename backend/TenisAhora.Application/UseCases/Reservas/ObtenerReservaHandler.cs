using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Reservas;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Reservas;

/// <summary>
/// Caso de Uso: Consultar Reserva por ID
/// [RF - 25] El sistema debe permitir consultar reservas de canchas.
/// ● El sistema debe permitir identificar la reserva mediante su ID.
/// ● El sistema debe permitir consultar la reserva mediante su ID.
/// </summary>
public class ObtenerReservaHandler
{
    private readonly IReservaRepository _reservaRepository;

    public ObtenerReservaHandler(IReservaRepository reservaRepository)
    {
        _reservaRepository = reservaRepository;
    }

    public async Task<ReservaResponseDto> Handle(int id, CancellationToken cancellationToken = default)
    {
        // [RF - 25] Identificar y consultar la reserva mediante su ID
        var reserva = await _reservaRepository.ObtenerPorIdAsync(id, cancellationToken);
        if (reserva == null)
        {
            throw new DomainException($"No se encontró la reserva con ID {id}.");
        }

        var titularDni = reserva.Socios.FirstOrDefault()?.Dni ?? 0;
        var jugadoresDni = reserva.Socios.Select(s => s.Dni).ToList();
        var montoSeña = (decimal)reserva.importe * 0.5m;

        var equipamientos = reserva.DetallesStockReserva.Select(d => new DetalleEquipamientoResponseDto(
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
            (float)montoSeña,
            reserva.estado.ToString(),
            true,
            equipamientos
        );
    }
}
