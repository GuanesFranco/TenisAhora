using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Disponibilidad.Commands;

public record CrearDisponibilidadCommand(
    int CanchaId,
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin,
    EstadoDisponibilidad Estado
);