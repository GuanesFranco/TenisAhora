using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.DTOs.Recibos;
using TenisAhora.Application.UseCases.Recibos.Commands;
using TenisAhora.Application.UseCases.Recibos.Handlers;
using TenisAhora.Application.UseCases.Recibos.Queries;

namespace TenisAhora.Api.Controllers;

/// <summary>
/// Controlador para la Gestión y Emisión de Recibos.
/// Cumple con las especificaciones [RF-69, RF-70, RF-71, RF-72].
/// </summary>
[ApiController]
[Route("api/recibos")]
public class RecibosController : ControllerBase
{
    private readonly GenerarReciboHandler _generarHandler;
    private readonly ObtenerReciboPorIdHandler _obtenerPorIdHandler;
    private readonly ListarRecibosHandler _listarHandler;
    private readonly AnularReciboHandler _anularHandler;
    private readonly ImprimirReciboHandler _imprimirHandler;

    public RecibosController(
        GenerarReciboHandler generarHandler,
        ObtenerReciboPorIdHandler obtenerPorIdHandler,
        ListarRecibosHandler listarHandler,
        AnularReciboHandler anularHandler,
        ImprimirReciboHandler imprimirHandler)
    {
        _generarHandler = generarHandler;
        _obtenerPorIdHandler = obtenerPorIdHandler;
        _listarHandler = listarHandler;
        _anularHandler = anularHandler;
        _imprimirHandler = imprimirHandler;
    }

    /// <summary>
    /// [RF-69] Genera un nuevo recibo para un cobro validado.
    /// </summary>
    [HttpPost("generar/{numeroCobro}")]
    public async Task<ActionResult<ReciboResponseDto>> Generar(int numeroCobro, CancellationToken ct)
    {
        var creado = await _generarHandler.Handle(new GenerarReciboCommand(numeroCobro), ct);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    /// <summary>
    /// [RF-70] Consulta un recibo emitido por su identificador.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ReciboResponseDto>> ObtenerPorId(int id, CancellationToken ct)
    {
        var recibo = await _obtenerPorIdHandler.Handle(new ObtenerReciboPorIdQuery(id), ct);
        return Ok(recibo);
    }

    /// <summary>
    /// [RF-70] Consulta el listado general de todos los recibos emitidos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ReciboResponseDto>>> Listar(CancellationToken ct)
    {
        var lista = await _listarHandler.Handle(new ListarRecibosQuery(), ct);
        return Ok(lista);
    }

    /// <summary>
    /// [RF-71] Anula un recibo emitido previamente.
    /// </summary>
    [HttpPost("{id}/anular")]
    public async Task<ActionResult<ReciboResponseDto>> Anular(int id, CancellationToken ct)
    {
        var recibo = await _anularHandler.Handle(new AnularReciboCommand(id), ct);
        return Ok(recibo);
    }

    /// <summary>
    /// [RF-72] Genera el detalle técnico del recibo para su impresión física o emisión digital.
    /// </summary>
    [HttpGet("{id}/imprimir")]
    public async Task<ActionResult<ImpresionReciboDto>> Imprimir(int id, CancellationToken ct)
    {
        var impresion = await _imprimirHandler.Handle(new ImprimirReciboQuery(id), ct);
        return Ok(impresion);
    }
}
