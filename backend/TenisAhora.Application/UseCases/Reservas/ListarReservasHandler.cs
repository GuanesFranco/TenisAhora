using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Reservas;
using TenisAhora.Application.Interfaces;

namespace TenisAhora.Application.UseCases.Reservas;

/// <summary>
/// Caso de Uso: Consultar Listado General de Reservas
/// [RF - 25] El sistema debe permitir consultar reservas de canchas.
/// </summary>
public class ListarReservasHandler
{
    private readonly IReservaRepository _reservaRepository;

    public ListarReservasHandler(IReservaRepository reservaRepository)
    {
        _reservaRepository = reservaRepository;
    }

    public async Task<List<ReservaResponseDto>> Handle(CancellationToken cancellationToken = default)
    {
        // [RF - 25] Permite consultar el listado general de reservas registradas en el sistema
        var reservas = await _reservaRepository.ObtenerTodasAsync(cancellationToken);

        return reservas.Select(r =>
        {
            var titularDni = r.Socios.FirstOrDefault()?.Dni ?? 0;
            var jugadoresDni = r.Socios.Select(s => s.Dni).ToList();
            var montoSeña = (decimal)r.importe * 0.5m;

            var equipamientos = r.DetallesStockReserva.Select(d => new DetalleEquipamientoResponseDto(
                d.StockId,
                d.Stock?.tipo_elemento ?? string.Empty,
                d.cantidad
            )).ToList();

            return new ReservaResponseDto(
                r.Id,
                r.CanchaId,
                r.fecha_reserva,
                r.HoraInicio,
                r.HoraFin,
                titularDni,
                jugadoresDni,
                r.importe,
                (float)montoSeña,
                r.estado.ToString(),
                true,
                equipamientos
            );
        }).ToList();
    }
}
