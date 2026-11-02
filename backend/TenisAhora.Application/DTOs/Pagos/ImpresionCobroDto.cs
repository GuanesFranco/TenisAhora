using System;

namespace TenisAhora.Application.DTOs.Pagos;

public record ImpresionCobroDto(
    int NumeroCobro,
    string Establecimiento,
    DateTime FechaPago,
    string Concepto,
    float ImporteTotal,
    float PorcentajePago,
    string MedioDePago,
    string Estado,
    int? ReservaId,
    int? InscripcionId,
    long? MercadoPagoPaymentId,
    int? ReciboId
);
