using System;
using System.ComponentModel.DataAnnotations;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.DTOs.Disponibilidad;

/// <summary>
/// [RF - 18] DTO para modificar disponibilidad de una cancha.
/// </summary>
public record ModificarDisponibilidadCommand(
    [Required] DateTime FechaHoraInicio,
    [Required] DateTime FechaHoraFin,
    [Required] EstadoDisponibilidad Estado
);

/// <summary>
/// [RF - 17] Respuesta con los datos de una disponibilidad.
/// </summary>
public record DisponibilidadResponseDto(
    int Id,
    int CanchaId,
    string TipoCanchaSuperficie,
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin,
    EstadoDisponibilidad Estado,
    string EstadoNombre
);