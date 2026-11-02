using System;

namespace TenisAhora.Application.DTOs.Recibos;

public record ReciboResponseDto(
    int Id,
    int NumeroCobro,
    DateTime FechaEmision,
    float ImporteTotal,
    float PorcentajePago,
    string MedioDePago,
    bool Anulado
);
