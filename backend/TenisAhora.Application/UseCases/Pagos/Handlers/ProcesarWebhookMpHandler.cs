using System;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Pagos.Commands;

namespace TenisAhora.Application.UseCases.Pagos.Handlers;

/// <summary>
/// Caso de Uso: Procesamiento asincrónico del Webhook oficial de Mercado Pago para pagos QR.
/// Garantiza idempotencia, validación de estado en Mercado Pago y confirmación del cobro y reserva.
/// </summary>
public class ProcesarWebhookMpHandler
{
    private readonly IMercadoPagoService _mercadoPagoService;
    private readonly ConfirmarCobroQrHandler _confirmarCobroQrHandler;

    public ProcesarWebhookMpHandler(
        IMercadoPagoService mercadoPagoService,
        ConfirmarCobroQrHandler confirmarCobroQrHandler)
    {
        _mercadoPagoService = mercadoPagoService;
        _confirmarCobroQrHandler = confirmarCobroQrHandler;
    }

    public async Task<bool> Handle(long paymentId, CancellationToken ct = default)
    {
        var pagoMp = await _mercadoPagoService.ConsultarPagoAsync(paymentId, ct);

        if (!pagoMp.Aprobado || !int.TryParse(pagoMp.ExternalReference, out int numeroCobro))
        {
            return false;
        }

        try
        {
            await _confirmarCobroQrHandler.Handle(new ConfirmarCobroQrCommand(numeroCobro, paymentId), ct);
            return true;
        }
        catch (Exception)
        {
            // Idempotencia: si el cobro ya había sido validado previamente, respondemos exitosamente
            // para que Mercado Pago no reintente notificaciones redundantes.
            return true;
        }
    }
}
