using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.DTOs.Disponibilidad;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Disponibilidad.Commands;
using TenisAhora.Application.UseCases.Disponibilidad.Queries;

namespace TenisAhora.Api.Controllers;

/// <summary>
/// Controlador para la Gestión de Disponibilidad de Canchas.
/// Cumple con las especificaciones [RF - 16], [RF - 17], [RF - 18].
/// </summary>
[ApiController]
[Route("api/canchas/{canchaId:int}/disponibilidad")]
public class DisponibilidadController : ControllerBase
{
    private readonly ICrearDisponibilidadHandler _crearHandler;
    private readonly IModificarDisponibilidadHandler _modificarHandler;
    private readonly IGetDisponibilidadByCanchaHandler _getByCanchaHandler;
    private readonly IGetDisponibilidadByIdHandler _getByIdHandler;

    public DisponibilidadController(
        ICrearDisponibilidadHandler crearHandler,
        IModificarDisponibilidadHandler modificarHandler,
        IGetDisponibilidadByCanchaHandler getByCanchaHandler,
        IGetDisponibilidadByIdHandler getByIdHandler)
    {
        _crearHandler = crearHandler;
        _modificarHandler = modificarHandler;
        _getByCanchaHandler = getByCanchaHandler;
        _getByIdHandler = getByIdHandler;
    }

    /// <summary>
    /// [RF - 16] Registra la disponibilidad de una cancha (horarios disponibles o bloqueos por mantenimiento).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DisponibilidadResponseDto>> Crear(
        [FromRoute] int canchaId,
        [FromBody] CrearDisponibilidadCommand command,
        CancellationToken cancellationToken)
    {
        if (canchaId != command.CanchaId)
        {
            return BadRequest("El ID de la cancha en la URL no coincide con el del cuerpo de la petición.");
        }

        var disponibilidad = await _crearHandler.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { canchaId, id = disponibilidad.Id }, disponibilidad);
    }

    /// <summary>
    /// [RF - 17] Consulta la disponibilidad de una cancha con filtros opcionales.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DisponibilidadResponseDto>>> ObtenerPorCancha(
        [FromRoute] int canchaId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] TenisAhora.Domain.Enums.EstadoDisponibilidad? estado,
        CancellationToken cancellationToken)
    {
        var query = new GetDisponibilidadByCanchaQuery(canchaId, fechaDesde, fechaHasta, estado);
        var disponibilidades = await _getByCanchaHandler.Handle(query, cancellationToken);
        return Ok(disponibilidades);
    }

    /// <summary>
    /// [RF - 17] Consulta una disponibilidad específica por su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DisponibilidadResponseDto>> ObtenerPorId(
        [FromRoute] int canchaId,
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var query = new GetDisponibilidadByIdQuery(id);
        var disponibilidad = await _getByIdHandler.Handle(query, cancellationToken);

        if (disponibilidad is null)
        {
            return NotFound(new { mensaje = $"No se encontró una disponibilidad con ID {id}." });
        }

        if (disponibilidad.CanchaId != canchaId)
        {
            return NotFound(new { mensaje = $"La disponibilidad con ID {id} no pertenece a la cancha {canchaId}." });
        }

        return Ok(disponibilidad);
    }

    /// <summary>
    /// [RF - 18] Modifica la disponibilidad de una cancha (horarios o estado).
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DisponibilidadResponseDto>> Modificar(
        [FromRoute] int canchaId,
        [FromRoute] int id,
        [FromBody] ModificarDisponibilidadCommand command,
        CancellationToken cancellationToken)
    {
        var disponibilidad = await _modificarHandler.Handle(id, command, cancellationToken);

        if (disponibilidad is null)
        {
            return NotFound(new { mensaje = $"No se encontró una disponibilidad con ID {id}." });
        }

        if (disponibilidad.CanchaId != canchaId)
        {
            return NotFound(new { mensaje = $"La disponibilidad con ID {id} no pertenece a la cancha {canchaId}." });
        }

        return Ok(disponibilidad);
    }
}