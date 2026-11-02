using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.UseCases.Ligas;

namespace TenisAhora.Api.Controllers;

[ApiController]
[Route("api/ligas")]
public class LigasController : ControllerBase
{
    private readonly CrearLigaHandler _crearHandler;
    private readonly ObtenerLigaHandler _obtenerHandler;
    private readonly ListarLigasHandler _listarHandler;
    private readonly ModificarLigaHandler _modificarHandler;
    private readonly EliminarLigaHandler _eliminarHandler;
    private readonly InscribirLigaHandler _inscribirHandler;
    private readonly GenerarFixtureLigaHandler _fixtureHandler;
    private readonly RegistrarResultadoLigaHandler _resultadoHandler;
    private readonly ObtenerTablaPosicionesHandler _posicionesHandler;

    public LigasController(
        CrearLigaHandler crearHandler,
        ObtenerLigaHandler obtenerHandler,
        ListarLigasHandler listarHandler,
        ModificarLigaHandler modificarHandler,
        EliminarLigaHandler eliminarHandler,
        InscribirLigaHandler inscribirHandler,
        GenerarFixtureLigaHandler fixtureHandler,
        RegistrarResultadoLigaHandler resultadoHandler,
        ObtenerTablaPosicionesHandler posicionesHandler)
    {
        _crearHandler = crearHandler;
        _obtenerHandler = obtenerHandler;
        _listarHandler = listarHandler;
        _modificarHandler = modificarHandler;
        _eliminarHandler = eliminarHandler;
        _inscribirHandler = inscribirHandler;
        _fixtureHandler = fixtureHandler;
        _resultadoHandler = resultadoHandler;
        _posicionesHandler = posicionesHandler;
    }

    /// <summary>
    /// Consulta el listado general de ligas (RF-36)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<LigaResponseDto>>> Listar([FromQuery] bool soloActivos = true)
    {
        var ligas = await _listarHandler.Handle(soloActivos);
        return Ok(ligas);
    }

    /// <summary>
    /// Consulta los datos completos, participantes y calendario de una liga por ID (RF-36)
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<LigaResponseDto>> ObtenerPorId([FromRoute] int id)
    {
        var liga = await _obtenerHandler.Handle(id);
        return Ok(liga);
    }

    /// <summary>
    /// Registra una nueva liga regular (RF-35)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<LigaResponseDto>> Registrar([FromBody] CrearLigaCommand command)
    {
        var nuevaLiga = await _crearHandler.Handle(command);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevaLiga.Id }, nuevaLiga);
    }

    /// <summary>
    /// Modifica los parámetros de una liga existente (RF-37)
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<LigaResponseDto>> Modificar([FromRoute] int id, [FromBody] ModificarLigaCommand command)
    {
        var ligaModificada = await _modificarHandler.Handle(id, command);
        return Ok(ligaModificada);
    }

    /// <summary>
    /// Elimina o da de baja una liga si no está en disputa activa (RF-38)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
    {
        await _eliminarHandler.Handle(id);
        return NoContent();
    }

    /// <summary>
    /// Inscribe a un socio en la liga mediante su DNI (RF-39)
    /// </summary>
    [HttpPost("{id:int}/inscripciones")]
    public async Task<ActionResult<InscriptoDto>> Inscribir([FromRoute] int id, [FromBody] InscribirLigaCommand command)
    {
        var inscripto = await _inscribirHandler.Handle(id, command);
        return StatusCode(StatusCodes.Status201Created, inscripto);
    }

    /// <summary>
    /// Genera automáticamente el fixture "todos contra todos" (Round Robin) para la liga (RF-40)
    /// </summary>
    [HttpPost("{id:int}/fixture")]
    public async Task<ActionResult<List<PartidoDto>>> GenerarFixture([FromRoute] int id)
    {
        var partidos = await _fixtureHandler.Handle(id);
        return Ok(partidos);
    }

    /// <summary>
    /// Registra el resultado de un partido de la liga (RF-41)
    /// </summary>
    [HttpPost("partidos/{partidoId:int}/resultado")]
    public async Task<ActionResult<PartidoDto>> RegistrarResultado(
        [FromRoute] int partidoId,
        [FromBody] RegistrarResultadoPartidoCommand command)
    {
        var partido = await _resultadoHandler.Handle(partidoId, command);
        return Ok(partido);
    }

    /// <summary>
    /// Consulta la tabla de posiciones actualizada con cálculo automático de puntos (RF-36 y RF-41)
    /// </summary>
    [HttpGet("{id:int}/posiciones")]
    public async Task<ActionResult<List<TablaPosicionesItemDto>>> ObtenerTablaPosiciones([FromRoute] int id)
    {
        var posiciones = await _posicionesHandler.Handle(id);
        return Ok(posiciones);
    }
}
