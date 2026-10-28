using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.DTOs.Reservas;

/// <summary>
/// [RF - 24] Permite agregar equipamiento a la reserva según el stock disponible.
/// </summary>
public record EquipamientoSolicitadoDto(
    [Required] int EquipamientoId,
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")] int Cantidad
);

/// <summary>
/// [RF - 24] Comando para registrar una nueva reserva de cancha con sus participantes y equipamiento.
/// </summary>
public record CrearReservaCommand(
    [Required] int CanchaId,
    [Required] DateTime fecha_reserva,
    [Required] DateTime HoraInicio,
    [Required] DateTime HoraFin,
    [Required] int SocioDni,
    [Required] List<int> JugadoresDni,
    [Range(0.01, double.MaxValue, ErrorMessage = "El importe total debe ser mayor a cero.")] float ImporteTotal,
    List<EquipamientoSolicitadoDto>? EquipamientosSolicitados = null
);

/// <summary>
/// [RF - 26] Comando para modificar los datos de una reserva existente.
/// </summary>
public record ModificarReservaCommand(
    [Required] int CanchaId,
    [Required] DateTime fecha_reserva,
    [Required] DateTime HoraInicio,
    [Required] DateTime HoraFin,
    [Required] List<int> JugadoresDni,
    [Range(0.01, double.MaxValue, ErrorMessage = "El importe total debe ser mayor a cero.")] float ImporteTotal,
    List<EquipamientoSolicitadoDto>? EquipamientosSolicitados = null
);

/// <summary>
/// [RF - 24] Comando para registrar y validar el pago de la seña (50%) requerido para confirmar la reserva.
/// </summary>
public record RegistrarPagoSenaCommand(
    [Range(0.01, double.MaxValue, ErrorMessage = "El importe del pago debe ser mayor a cero.")] float ImportePago,
    [Required] TipoDePago TipoPago
);

/// <summary>
/// [RF - 27] DTO para consultar o configurar el plazo mínimo de antelación requerido para cancelaciones.
/// </summary>
public record ConfiguracionAntelacionDto(
    [Range(0, 720, ErrorMessage = "El plazo mínimo de antelación debe ser entre 0 y 720 horas.")] double HorasMinimasAntelacion
);

/// <summary>
/// Detalle del equipamiento reservado y descontado del stock.
/// </summary>
public record DetalleEquipamientoResponseDto(
    int StockId,
    string TipoElemento,
    int Cantidad
);

/// <summary>
/// [RF - 25] Respuesta con los datos completos de una reserva de cancha.
/// </summary>
public record ReservaResponseDto(
    int Id,
    int CanchaId,
    DateTime fecha_reserva,
    DateTime HoraInicio,
    DateTime HoraFin,
    int SocioDni,
    List<int> JugadoresDni,
    float ImporteTotal,
    float MontoSena,
    string Estado,
    bool Activo,
    List<DetalleEquipamientoResponseDto>? Equipamientos = null
);

/// <summary>
/// [RF - 27] Respuesta con el resultado del procesamiento de la cancelación y reembolso.
/// </summary>
public record CancelarReservaResponseDto(
    int ReservaId,
    string Mensaje,
    bool AplicaReembolso,
    string Estado
);