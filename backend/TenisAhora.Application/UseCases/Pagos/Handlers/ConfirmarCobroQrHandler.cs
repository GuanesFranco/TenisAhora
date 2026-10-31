using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Pagos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Pagos.Commands;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Pagos.Handlers;

/// <summary>
/// Caso de Uso: Confirmar Cobro QR mediante Notificación o Consulta de Pago [RF-65, RD-08]
/// </summary>
public class ConfirmarCobroQrHandler
{
    private readonly IPagoRepository _pagoRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly IStockReservasRepository _stockRepository;
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly IReciboRepository _reciboRepository;

    public ConfirmarCobroQrHandler(
        IPagoRepository pagoRepository,
        IReservaRepository reservaRepository,
        IStockReservasRepository stockRepository,
        IMercadoPagoService mercadoPagoService,
        IReciboRepository reciboRepository)
    {
        _pagoRepository = pagoRepository;
        _reservaRepository = reservaRepository;
        _stockRepository = stockRepository;
        _mercadoPagoService = mercadoPagoService;
        _reciboRepository = reciboRepository;
    }

    public async Task<CobroResponseDto> Handle(ConfirmarCobroQrCommand command, CancellationToken ct = default)
    {
        var pago = await _pagoRepository.ObtenerPorIdAsync(command.NumeroCobro, ct);
        if (pago is null)
        {
            throw new DomainException($"No se encontró el cobro con número {command.NumeroCobro}.");
        }

        if (pago.tipo_pago != TipoDePago.QR || pago.estado != EstadoPago.Pendiente)
        {
            throw new DomainException("El cobro no corresponde a un pago QR pendiente.");
        }

        var resultadoMp = await _mercadoPagoService.ConsultarPagoAsync(command.MercadoPagoPaymentId, ct);
        if (!resultadoMp.Aprobado)
        {
            throw new DomainException($"El pago en Mercado Pago no se encuentra aprobado (Estado: {resultadoMp.Status}).");
        }

        pago.estado = EstadoPago.Validado;
        pago.MercadoPagoPaymentId = resultadoMp.PaymentId;
        await _pagoRepository.ActualizarAsync(pago, ct);

        if (pago.Recibo == null)
        {
            var reciboExistente = await _reciboRepository.ObtenerPorPagoIdAsync(pago.Id, ct);
            if (reciboExistente == null)
            {
                var nuevoRecibo = new Recibo
                {
                    PagoId = pago.Id,
                    FechaEmision = DateTime.UtcNow,
                    Anulado = false
                };
                await _reciboRepository.AgregarAsync(nuevoRecibo, ct);
                pago.Recibo = nuevoRecibo;
            }
            else
            {
                pago.Recibo = reciboExistente;
            }
        }

        if (pago.Reserva != null)
        {
            if (pago.porcentaje_pago == 50f && pago.Reserva.estado == EstadoReserva.Pendiente)
            {
                pago.Reserva.estado = EstadoReserva.Confirmada;
                await _reservaRepository.ActualizarReservaAsync(pago.Reserva, ct);
            }
            else if (pago.porcentaje_pago == 50f && pago.Reserva.estado == EstadoReserva.Confirmada)
            {
                foreach (var d in pago.Reserva.DetallesStockReserva)
                {
                    await _stockRepository.IncrementarStockAsync(d.StockId, d.cantidad, ct);
                }
            }
        }

        var titularDni = pago.Reserva?.Socios?.FirstOrDefault()?.Dni ?? 0;

        return new CobroResponseDto(
            pago.Id,
            titularDni,
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
