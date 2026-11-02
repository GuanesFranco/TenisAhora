using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.DTOs.Stock;
using TenisAhora.Application.UseCases.Stock.Commands;
using TenisAhora.Application.UseCases.Stock.Handlers;
using TenisAhora.Application.UseCases.Stock.Queries;

namespace TenisAhora.Api.Controllers;

/// <summary>
/// Controlador para la Gestión Integral de Stock de Equipamiento Deportivo.
/// Cumple con las especificaciones [RF-19 a RF-23] y [RD-14 a RD-16].
/// </summary>
[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly RegistrarStockHandler _registrarHandler;
    private readonly ListarStockHandler _listarHandler;
    private readonly ObtenerStockPorIdHandler _obtenerPorIdHandler;
    private readonly ModificarStockHandler _modificarHandler;
    private readonly EliminarStockHandler _eliminarHandler;
    private readonly FinalizarAlquilerStockHandler _finalizarAlquilerHandler;

    public StockController(
        RegistrarStockHandler registrarHandler,
        ListarStockHandler listarHandler,
        ObtenerStockPorIdHandler obtenerPorIdHandler,
        ModificarStockHandler modificarHandler,
        EliminarStockHandler eliminarHandler,
        FinalizarAlquilerStockHandler finalizarAlquilerHandler)
    {
        _registrarHandler = registrarHandler;
        _listarHandler = listarHandler;
        _obtenerPorIdHandler = obtenerPorIdHandler;
        _modificarHandler = modificarHandler;
        _eliminarHandler = eliminarHandler;
        _finalizarAlquilerHandler = finalizarAlquilerHandler;
    }

    /// <summary>
    /// [RF-19] Registra un nuevo equipamiento en el stock del club.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<StockResponseDto>> Registrar([FromBody] RegistrarStockCommand command, CancellationToken cancellationToken)
    {
        var creado = await _registrarHandler.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    /// <summary>
    /// [RF-20] Consulta el listado general de todo el equipamiento deportivo y su disponibilidad.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<StockResponseDto>>> Listar(CancellationToken cancellationToken)
    {
        var lista = await _listarHandler.Handle(new ListarStockQuery(), cancellationToken);
        return Ok(lista);
    }

    /// <summary>
    /// [RF-20] Consulta los datos y disponibilidad de un equipamiento por ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<StockResponseDto>> ObtenerPorId(int id, CancellationToken cancellationToken)
    {
        var stock = await _obtenerPorIdHandler.Handle(new ObtenerStockPorIdQuery(id), cancellationToken);
        return Ok(stock);
    }

    /// <summary>
    /// [RF-22, RD-15] Modifica la cantidad disponible de un equipamiento deportivo.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<StockResponseDto>> Modificar(int id, [FromBody] ModificarStockCommand command, CancellationToken cancellationToken)
    {
        var cmd = command with { Id = id };
        var actualizado = await _modificarHandler.Handle(cmd, cancellationToken);
        return Ok(actualizado);
    }

    /// <summary>
    /// [RF-23] Elimina un equipamiento del stock si no tiene reservas asociadas.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _eliminarHandler.Handle(new EliminarStockCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// [RF-21, RD-16] Finaliza el alquiler de stock asociado a una reserva y restituye las unidades al inventario.
    /// </summary>
    [HttpPost("reservas/{reservaId}/finalizar")]
    public async Task<IActionResult> FinalizarAlquiler(int reservaId, CancellationToken cancellationToken)
    {
        await _finalizarAlquilerHandler.Handle(new FinalizarAlquilerStockCommand(reservaId), cancellationToken);
        return Ok(new { mensaje = "Alquiler de equipamiento finalizado y stock restituido correctamente." });
    }
}
