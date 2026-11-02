using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Pagos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Pagos.Queries;

namespace TenisAhora.Application.UseCases.Pagos.Handlers;

/// <summary>
/// Caso de Uso: Listar Todos los Cobros Registrados [RF-66]
/// </summary>
public class ListarCobrosHandler
{
    private readonly IPagoRepository _pagoRepository;

    public ListarCobrosHandler(IPagoRepository pagoRepository)
    {
        _pagoRepository = pagoRepository;
    }

    public async Task<List<CobroResponseDto>> Handle(ListarCobrosQuery query, CancellationToken ct = default)
    {
        var pagos = await _pagoRepository.ListarTodosAsync(ct);

        return pagos.Select(pago =>
        {
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
        }).ToList();
    }
}
