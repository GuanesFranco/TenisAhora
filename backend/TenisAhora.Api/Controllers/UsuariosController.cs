using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.DTOs.Usuarios;
using TenisAhora.Application.UseCases.Usuarios.AsignarEntrenador;
using TenisAhora.Application.UseCases.Usuarios.AsignarProfesor;
using TenisAhora.Application.UseCases.Usuarios.CrearUsuario;
using TenisAhora.Application.UseCases.Usuarios.EliminarUsuario;
using TenisAhora.Application.UseCases.Usuarios.ListarUsuarios;
using TenisAhora.Application.UseCases.Usuarios.ModificarUsuario;
using TenisAhora.Application.UseCases.Usuarios.ObtenerFichaImpresion;
using TenisAhora.Application.UseCases.Usuarios.ObtenerUsuario;

namespace TenisAhora.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly CrearUsuarioCommandHandler _crearHandler;
    private readonly ObtenerUsuarioPorDniQueryHandler _obtenerHandler;
    private readonly ListarUsuariosQueryHandler _listarHandler;
    private readonly ModificarUsuarioCommandHandler _modificarHandler;
    private readonly EliminarUsuarioCommandHandler _eliminarHandler;
    private readonly ObtenerFichaImpresionQueryHandler _fichaHandler;
    private readonly AsignarProfesorCommandHandler _asignarProfesorHandler;
    private readonly AsignarEntrenadorCommandHandler _asignarEntrenadorHandler;

    public UsuariosController(
        CrearUsuarioCommandHandler crearHandler,
        ObtenerUsuarioPorDniQueryHandler obtenerHandler,
        ListarUsuariosQueryHandler listarHandler,
        ModificarUsuarioCommandHandler modificarHandler,
        EliminarUsuarioCommandHandler eliminarHandler,
        ObtenerFichaImpresionQueryHandler fichaHandler,
        AsignarProfesorCommandHandler asignarProfesorHandler,
        AsignarEntrenadorCommandHandler asignarEntrenadorHandler)
    {
        _crearHandler = crearHandler;
        _obtenerHandler = obtenerHandler;
        _listarHandler = listarHandler;
        _modificarHandler = modificarHandler;
        _eliminarHandler = eliminarHandler;
        _fichaHandler = fichaHandler;
        _asignarProfesorHandler = asignarProfesorHandler;
        _asignarEntrenadorHandler = asignarEntrenadorHandler;
    }

    /// <summary>
    /// Consulta y lista todos los usuarios (RF-03)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UsuarioResponseDto>>> Listar([FromQuery] bool soloActivos = true)
    {
        var usuarios = await _listarHandler.Handle(new ListarUsuariosQuery(soloActivos));
        return Ok(usuarios);
    }

    /// <summary>
    /// Consulta los datos de un usuario por su DNI (RF-03)
    /// </summary>
    [HttpGet("{dni:int}")]
    public async Task<ActionResult<UsuarioResponseDto>> ObtenerPorDni([FromRoute] int dni)
    {
        var usuario = await _obtenerHandler.Handle(new ObtenerUsuarioPorDniQuery(dni));
        return Ok(usuario);
    }

    /// <summary>
    /// Registra un nuevo usuario con validación de DNI único (RF-01)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UsuarioResponseDto>> Registrar([FromBody] CrearUsuarioCommand command)
    {
        var nuevoUsuario = await _crearHandler.Handle(command);
        return CreatedAtAction(nameof(ObtenerPorDni), new { dni = nuevoUsuario.Dni }, nuevoUsuario);
    }

    /// <summary>
    /// Modifica los datos de un usuario identificado por DNI (RF-02)
    /// </summary>
    [HttpPut("{dni:int}")]
    public async Task<ActionResult<UsuarioResponseDto>> Modificar([FromRoute] int dni, [FromBody] ModificarUsuarioCommand command)
    {
        var usuarioModificado = await _modificarHandler.Handle(dni, command);
        return Ok(usuarioModificado);
    }

    /// <summary>
    /// Da de baja lógica a un usuario por su DNI (RF-04)
    /// </summary>
    [HttpDelete("{dni:int}")]
    public async Task<IActionResult> Eliminar([FromRoute] int dni)
    {
        await _eliminarHandler.Handle(new EliminarUsuarioCommand(dni));
        return NoContent();
    }

    /// <summary>
    /// Genera los datos y ficha técnica del usuario para su impresión física (RF-05)
    /// </summary>
    [HttpGet("{dni:int}/impresion")]
    public async Task<ActionResult<FichaUsuarioDto>> ObtenerFichaImpresion([FromRoute] int dni)
    {
        var ficha = await _fichaHandler.Handle(new ObtenerFichaImpresionQuery(dni));
        return Ok(ficha);
    }

    /// <summary>
    /// Asigna un profesor habilitado a un usuario para clase particular (RF-06)
    /// </summary>
    [HttpPost("{dni:int}/asignar-profesor")]
    public async Task<ActionResult<AsignacionResponseDto>> AsignarProfesor(
        [FromRoute] int dni,
        [FromBody] AsignarProfesorRequest request)
    {
        var command = new AsignarProfesorCommand(
            dni,
            request.ProfesorId,
            request.FechaHoraInicio,
            request.DuracionMinutos,
            request.Nivel,
            request.TipoDeClase
        );

        var resultado = await _asignarProfesorHandler.Handle(command);
        return Ok(resultado);
    }

    /// <summary>
    /// Asigna un entrenador a un usuario para entrenamiento particular (RF-07)
    /// </summary>
    [HttpPost("{dni:int}/asignar-entrenador")]
    public async Task<ActionResult<AsignacionResponseDto>> AsignarEntrenador(
        [FromRoute] int dni,
        [FromBody] AsignarEntrenadorRequest request)
    {
        var command = new AsignarEntrenadorCommand(
            dni,
            request.EntrenadorId,
            request.FechaHoraInicio,
            request.DuracionMinutos,
            request.Nivel,
            request.Objetivo
        );

        var resultado = await _asignarEntrenadorHandler.Handle(command);
        return Ok(resultado);
    }
}

public record AsignarProfesorRequest(
    int ProfesorId,
    DateTime FechaHoraInicio,
    int DuracionMinutos = 60,
    Domain.Enums.NivelActividad Nivel = Domain.Enums.NivelActividad.Principiante,
    Domain.Enums.TipoClase TipoDeClase = Domain.Enums.TipoClase.Tecnica
);

public record AsignarEntrenadorRequest(
    int EntrenadorId,
    DateTime FechaHoraInicio,
    int DuracionMinutos = 60,
    Domain.Enums.NivelActividad Nivel = Domain.Enums.NivelActividad.Principiante,
    Domain.Enums.ObjetivoEntrenamiento Objetivo = Domain.Enums.ObjetivoEntrenamiento.Tecnica
);
