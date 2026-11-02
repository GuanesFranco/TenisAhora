using TenisAhora.Application.DTOs.Recibos;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.UseCases.Recibos.Handlers;

internal static class ReciboMapper
{
    public static ReciboResponseDto MapearADto(Recibo recibo)
    {
        var pago = recibo.Pago;
        return new ReciboResponseDto(
            recibo.Id,
            recibo.PagoId,
            recibo.FechaEmision,
            pago?.importe ?? 0f,
            pago?.porcentaje_pago ?? 0f,
            pago?.tipo_pago.ToString() ?? string.Empty,
            recibo.Anulado
        );
    }
}
