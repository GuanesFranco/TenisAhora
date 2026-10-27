using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.DTOs.Reservas;
using TenisAhora.Application.UseCases.Reservas;

namespace TenisAhora.Api.Controllers;

/// <summary>
/// Controlador para la Gestión Integral de Reservas de Canchas y Equipamiento.
/// Cumple con las especificaciones [RF - 24], [RF - 25], [RF - 26] y [RF - 27].
/// </summary>
[ApiController]
[Route("api/reservas")]
public class ReservasController : ControllerBase
{
    private readonly CrearReservaHandler _crearHandler;
    private readonly ConfirmarReservaHandler _confirmarHandler;
    private readonly ModificarReservaHandler _modificarHandler;
    private readonly CancelarReservaHandler _cancelarHandler;
    private readonly FinalizarReservaHandler _finalizarHandler;
    private readonly ObtenerReservaHandler _obtenerHandler;
    private readonly ListarReservasHandler _listarHandler;

    public ReservasController(
        CrearReservaHandler crearHandler,
        ConfirmarReservaHandler confirmarHandler,
        ModificarReservaHandler modificarHandler,
        CancelarReservaHandler cancelarHandler,
        FinalizarReservaHandler finalizarHandler,
        ObtenerReservaHandler obtenerHandler,
        ListarReservasHandler listarHandler)
    {
        _crearHandler = crearHandler;
        _confirmarHandler = confirmarHandler;
        _modificarHandler = modificarHandler;
        _cancelarHandler = cancelarHandler;
        _finalizarHandler = finalizarHandler;
        _obtenerHandler = obtenerHandler;
        _listarHandler = listarHandler;
    }

    /// <summary>
    /// [RF - 25] Consulta el listado general de reservas registradas en el sistema.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ReservaResponseDto>>> Listar(CancellationToken cancellationToken)
    {
        var reservas = await _listarHandler.Handle(cancellationToken);
        return Ok(reservas);
    }

    /// <summary>
    /// [RF - 25] Permite consultar los datos completos de una reserva identificada mediante su ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservaResponseDto>> ObtenerPorId([FromRoute] int id, CancellationToken cancellationToken)
    {
        var reserva = await _obtenerHandler.Handle(id, cancellationToken);
        return Ok(reserva);
    }

    /// <summary>
    /// [RF - 24] Registra una nueva reserva de cancha con ID automático, participantes y equipamiento según disponibilidad.
    /// Queda en estado 'Pendiente' hasta validar el pago del 50%.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ReservaResponseDto>> Crear([FromBody] CrearReservaCommand command, CancellationToken cancellationToken)
    {
        var reserva = await _crearHandler.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = reserva.Id }, reserva);
    }

    /// <summary>
    /// [RF - 24] Confirma la reserva registrando y validando el pago del 50% del valor del alquiler.
    /// </summary>
    [HttpPost("{id:int}/confirmar")]
    public async Task<ActionResult<ReservaResponseDto>> Confirmar(
        [FromRoute] int id,
        [FromBody] RegistrarPagoSenaCommand command,
        CancellationToken cancellationToken)
    {
        var reservaConfirmada = await _confirmarHandler.Handle(id, command, cancellationToken);
        return Ok(reservaConfirmada);
    }

    /// <summary>
    /// [RF - 26] Permite identificar la reserva mediante su ID, modificar sus datos y guardar las modificaciones.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ReservaResponseDto>> Modificar(
        [FromRoute] int id,
        [FromBody] ModificarReservaCommand command,
        CancellationToken cancellationToken)
    {
        var reservaModificada = await _modificarHandler.Handle(id, command, cancellationToken);
        return Ok(reservaModificada);
    }

    /// <summary>
    /// [RF - 27] Cancela la reserva seleccionada, aplicando cargo/retención si es fuera de término o reembolso si es dentro del plazo.
    /// </summary>
    [HttpPost("{id:int}/cancelar")]
    public async Task<ActionResult<CancelarReservaResponseDto>> Cancelar([FromRoute] int id, CancellationToken cancellationToken)
    {
        var resultado = await _cancelarHandler.Handle(id, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Finaliza la reserva seleccionada y reintegra el equipamiento asignado al stock general.
    /// </summary>
    [HttpPost("{id:int}/finalizar")]
    public async Task<ActionResult<ReservaResponseDto>> Finalizar([FromRoute] int id, CancellationToken cancellationToken)
    {
        var resultado = await _finalizarHandler.Handle(id, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// [RF - 27] Consulta el plazo mínimo de antelación actualmente configurado para las cancelaciones.
    /// </summary>
    [HttpGet("configuracion/antelacion-cancelacion")]
    public ActionResult<ConfiguracionAntelacionDto> ObtenerPlazoAntelacion()
    {
        return Ok(new ConfiguracionAntelacionDto(CancelarReservaHandler.HorasMinimasAntelacion));
    }

    /// <summary>
    /// [RF - 27] Permite al personal autorizado configurar el plazo mínimo de antelación requerida para las cancelaciones.
    /// </summary>
    [HttpPut("configuracion/antelacion-cancelacion")]
    public ActionResult<ConfiguracionAntelacionDto> ConfigurarPlazoAntelacion([FromBody] ConfiguracionAntelacionDto dto)
    {
        if (dto.HorasMinimasAntelacion < 0)
        {
            return BadRequest("El plazo mínimo de antelación no puede ser negativo.");
        }

        CancelarReservaHandler.HorasMinimasAntelacion = dto.HorasMinimasAntelacion;
        return Ok(dto);
    }
}
