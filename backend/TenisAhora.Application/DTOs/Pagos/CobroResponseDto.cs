using System;

namespace TenisAhora.Application.DTOs.Pagos;

public record CobroResponseDto(
    int NumeroCobro,
    int SocioDni,
    float Importe,
    float PorcentajePago,
    string TipoPago,
    string Estado,
    DateTime FechaPago,
    int? ReservaId,
    int? InscripcionId,
    int? DescuentoId,
    long? MercadoPagoPaymentId,
    string? PreferenceIdQr = null,
    string? UrlPagoQr = null,
    string? CodigoQrBase64 = null,
    int? ReciboId = null
);
