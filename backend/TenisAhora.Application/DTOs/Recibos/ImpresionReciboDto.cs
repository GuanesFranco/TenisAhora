using System;

namespace TenisAhora.Application.DTOs.Recibos;

public record ImpresionReciboDto(
    int ReciboId,
    int NumeroCobro,
    string Establecimiento,
    DateTime FechaEmision,
    int? SocioDni,
    string? NombreSocio,
    string Concepto,
    float ImporteTotal,
    float PorcentajeAbonado,
    string MedioDePago,
    string EstadoRecibo
);
