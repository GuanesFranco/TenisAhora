using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Pagos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Pagos.Queries;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Pagos.Handlers;

/// <summary>
/// Caso de Uso: Obtener Cobro por Número [RF-66]
/// </summary>
public class ObtenerCobroPorNumeroHandler
{
    private readonly IPagoRepository _pagoRepository;

    public ObtenerCobroPorNumeroHandler(IPagoRepository pagoRepository)
    {
        _pagoRepository = pagoRepository;
    }

    public async Task<CobroResponseDto> Handle(ObtenerCobroPorNumeroQuery query, CancellationToken ct = default)
    {
        var pago = await _pagoRepository.ObtenerPorIdAsync(query.NumeroCobro, ct);
        if (pago is null)
        {
            throw new DomainException($"No se encontró el cobro con número {query.NumeroCobro}.");
        }

        var socioDni = pago.Reserva?.Socios?.FirstOrDefault()?.Dni ?? 0;

        return new CobroResponseDto(
            pago.Id,
            socioDni,
            pago.importe,
            pago.porcentaje_pago,
            pago.tipo_pago.ToString(),
            pago.estado.ToString(),
            pago.Fecha_pago,
            pago.ReservaId,
            pago.InscripcionId,
            pago.DescuentoId,
            pago.MercadoPagoPaymentId,
            null,
            null,
            null,
            pago.Recibo?.Id
        );
    }
}
