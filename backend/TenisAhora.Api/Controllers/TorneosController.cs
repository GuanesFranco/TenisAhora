using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.UseCases.Torneos;

namespace TenisAhora.Api.Controllers;

[ApiController]
[Route("api/torneos")]
public class TorneosController : ControllerBase
{
    private readonly CrearTorneoHandler _crearHandler;
    private readonly ObtenerTorneoHandler _obtenerHandler;
    private readonly ListarTorneosHandler _listarHandler;
    private readonly ModificarTorneoHandler _modificarHandler;
    private readonly EliminarTorneoHandler _eliminarHandler;
    private readonly InscribirTorneoHandler _inscribirHandler;
    private readonly GenerarFixtureTorneoHandler _fixtureHandler;
    private readonly RegistrarResultadoTorneoHandler _resultadoHandler;

    public TorneosController(
        CrearTorneoHandler crearHandler,
        ObtenerTorneoHandler obtenerHandler,
        ListarTorneosHandler listarHandler,
        ModificarTorneoHandler modificarHandler,
        EliminarTorneoHandler eliminarHandler,
        InscribirTorneoHandler inscribirHandler,
        GenerarFixtureTorneoHandler fixtureHandler,
        RegistrarResultadoTorneoHandler resultadoHandler)
    {
        _crearHandler = crearHandler;
        _obtenerHandler = obtenerHandler;
        _listarHandler = listarHandler;
        _modificarHandler = modificarHandler;
        _eliminarHandler = eliminarHandler;
        _inscribirHandler = inscribirHandler;
        _fixtureHandler = fixtureHandler;
        _resultadoHandler = resultadoHandler;
    }

    /// <summary>
    /// Consulta el listado general de torneos (RF-29)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<TorneoResponseDto>>> Listar([FromQuery] bool soloActivos = true)
    {
        var torneos = await _listarHandler.Handle(soloActivos);
        return Ok(torneos);
    }

    /// <summary>
    /// Consulta los datos completos, inscriptos y partidos de un torneo por su ID (RF-29)
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TorneoResponseDto>> ObtenerPorId([FromRoute] int id)
    {
        var torneo = await _obtenerHandler.Handle(id);
        return Ok(torneo);
    }

    /// <summary>
    /// Registra un nuevo torneo oficial del club (RF-28)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TorneoResponseDto>> Registrar([FromBody] CrearTorneoCommand command)
    {
        var nuevoTorneo = await _crearHandler.Handle(command);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoTorneo.Id }, nuevoTorneo);
    }

    /// <summary>
    /// Modifica los parámetros de un torneo (RF-30)
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TorneoResponseDto>> Modificar([FromRoute] int id, [FromBody] ModificarTorneoCommand command)
    {
        var torneoModificado = await _modificarHandler.Handle(id, command);
        return Ok(torneoModificado);
    }

    /// <summary>
    /// Elimina o da de baja un torneo si no tiene partidos disputados (RF-31)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar([FromRoute] int id)
    {
        await _eliminarHandler.Handle(id);
        return NoContent();
    }

    /// <summary>
    /// Inscribe a un socio en el torneo mediante su DNI (RF-32)
    /// </summary>
    [HttpPost("{id:int}/inscripciones")]
    public async Task<ActionResult<InscriptoDto>> Inscribir([FromRoute] int id, [FromBody] InscribirTorneoCommand command)
    {
        var inscripto = await _inscribirHandler.Handle(id, command);
        return StatusCode(StatusCodes.Status201Created, inscripto);
    }

    /// <summary>
    /// Genera automáticamente el cuadro de llaves/cruces eliminatorios del torneo (RF-33)
    /// </summary>
    [HttpPost("{id:int}/fixture")]
    public async Task<ActionResult<List<PartidoDto>>> GenerarFixture([FromRoute] int id)
    {
        var partidos = await _fixtureHandler.Handle(id);
        return Ok(partidos);
    }

    /// <summary>
    /// Registra los resultados oficiales de un partido del torneo (RF-34)
    /// </summary>
    [HttpPost("partidos/{partidoId:int}/resultado")]
    public async Task<ActionResult<PartidoDto>> RegistrarResultado(
        [FromRoute] int partidoId,
        [FromBody] RegistrarResultadoPartidoCommand command)
    {
        var partido = await _resultadoHandler.Handle(partidoId, command);
        return Ok(partido);
    }
}
