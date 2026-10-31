using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Pagos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Pagos.Commands;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Pagos.Handlers;

/// <summary>
/// Caso de Uso: Anular Cobro y Reembolsar/Cancelar en Mercado Pago [RF-67]
/// </summary>
public class AnularCobroHandler
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IMercadoPagoService _mercadoPagoService;

    public AnularCobroHandler(
        IPagoRepository pagoRepository,
        IMercadoPagoService mercadoPagoService)
    {
        _pagoRepository = pagoRepository;
        _mercadoPagoService = mercadoPagoService;
    }

    public async Task<CobroResponseDto> Handle(AnularCobroCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Motivo))
        {
            throw new DomainException("Es obligatorio registrar el motivo de la anulación del cobro.");
        }

        var pago = await _pagoRepository.ObtenerPorIdAsync(command.NumeroCobro, ct);
        if (pago is null)
        {
            throw new DomainException($"No se encontró el cobro con número {command.NumeroCobro}.");
        }

        if (pago.estado == EstadoPago.Anulado)
        {
            throw new DomainException("El cobro ya se encuentra anulado.");
        }

        if (pago.MercadoPagoPaymentId.HasValue)
        {
            await _mercadoPagoService.AnularOReembolsarPagoAsync(pago.MercadoPagoPaymentId.Value, ct);
        }

        pago.estado = EstadoPago.Anulado;
        if (pago.Recibo != null)
        {
            pago.Recibo.Anulado = true;
        }

        await _pagoRepository.ActualizarAsync(pago, ct);

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
