namespace TenisAhora.Application.UseCases.Disponibilidad.Queries;

public record GetDisponibilidadByCanchaQuery(
    int CanchaId,
    DateTime? FechaDesde = null,
    DateTime? FechaHasta = null,
    TenisAhora.Domain.Enums.EstadoDisponibilidad? Estado = null
);