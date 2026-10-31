using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Error;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
using QRCoder;
using TenisAhora.Application.DTOs.Pagos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Infrastructure.Services;

public class MercadoPagoService : IMercadoPagoService
{
    public MercadoPagoService(IConfiguration configuration)
    {
        var accessToken = configuration["MercadoPago:AccessToken"];
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new DomainException("Falta configurar MercadoPago:AccessToken en appsettings.json.");
        }

        MercadoPagoConfig.AccessToken = accessToken;
    }

    public async Task<ResultadoPagoMpDto> ProcesarPagoTarjetaAsync(ProcesarTarjetaMpRequest request, CancellationToken ct = default)
    {
        var client = new PaymentClient();

        var paymentRequest = new PaymentCreateRequest
        {
            TransactionAmount = request.Monto,
            Token = request.TokenTarjeta,
            Description = request.Descripcion,
            Installments = request.Cuotas,
            PaymentMethodId = request.PaymentMethodId,
            Payer = new PaymentPayerRequest
            {
                Email = request.EmailPagador,
                Identification = new IdentificationRequest
                {
                    Type = "DNI",
                    Number = request.DniPagador.ToString()
                }
            }
        };

        try
        {
            Payment payment = await client.CreateAsync(paymentRequest, cancellationToken: ct);
            return new ResultadoPagoMpDto(
                payment.Id ?? 0,
                payment.Status ?? string.Empty,
                payment.StatusDetail ?? string.Empty,
                payment.Status == "approved"
            );
        }
        catch (MercadoPagoApiException ex)
        {
            throw new DomainException($"Error al procesar pago en Mercado Pago: {ex.ApiError?.Message ?? ex.Message}");
        }
    }

    public async Task<ResultadoQrMpDto> GenerarQrPagoAsync(GenerarQrMpRequest request, CancellationToken ct = default)
    {
        var preferenceClient = new PreferenceClient();

        var prefRequest = new PreferenceRequest
        {
            ExternalReference = request.ReferenciaExterna,
            Payer = new PreferencePayerRequest
            {
                Email = request.EmailPagador
            },
            Items = new List<PreferenceItemRequest>
            {
                new PreferenceItemRequest
                {
                    Title = request.TituloConcepto,
                    Quantity = 1,
                    CurrencyId = "ARS",
                    UnitPrice = request.Monto
                }
            }
        };

        try
        {
            Preference pref = await preferenceClient.CreateAsync(prefRequest, cancellationToken: ct);
            var urlQr = !string.IsNullOrEmpty(pref.SandboxInitPoint) ? pref.SandboxInitPoint : pref.InitPoint;

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(urlQr, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var base64 = Convert.ToBase64String(qrCode.GetGraphic(10));

            return new ResultadoQrMpDto(pref.Id, urlQr, base64);
        }
        catch (MercadoPagoApiException ex)
        {
            throw new DomainException($"Error al generar QR en Mercado Pago: {ex.ApiError?.Message ?? ex.Message}");
        }
    }

    public async Task<ResultadoPagoMpDto> ConsultarPagoAsync(long paymentId, CancellationToken ct = default)
    {
        var client = new PaymentClient();

        try
        {
            var payment = await client.GetAsync(paymentId, cancellationToken: ct);
            return new ResultadoPagoMpDto(
                payment.Id ?? 0,
                payment.Status ?? string.Empty,
                payment.StatusDetail ?? string.Empty,
                payment.Status == "approved"
            );
        }
        catch (MercadoPagoApiException ex)
        {
            throw new DomainException($"Error al consultar pago en Mercado Pago: {ex.ApiError?.Message ?? ex.Message}");
        }
    }

    public async Task AnularOReembolsarPagoAsync(long paymentId, CancellationToken ct = default)
    {
        var client = new PaymentClient();

        try
        {
            var payment = await client.GetAsync(paymentId, cancellationToken: ct);

            if (payment.Status == "approved")
            {
                await client.RefundAsync(paymentId, cancellationToken: ct);
            }
            else if (payment.Status == "pending" || payment.Status == "in_process")
            {
                await client.CancelAsync(paymentId, cancellationToken: ct);
            }
        }
        catch (MercadoPagoApiException ex)
        {
            throw new DomainException($"Error al anular o reembolsar pago en Mercado Pago: {ex.ApiError?.Message ?? ex.Message}");
        }
    }
}
