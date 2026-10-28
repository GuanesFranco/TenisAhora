using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Entrenamientos.Commands;
using TenisAhora.Application.UseCases.Entrenamientos.Queries;

namespace TenisAhora.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntrenamientosController : ControllerBase
    {
        private readonly ICreateEntrenamientoHandler _createHandler;
        private readonly IGetEntrenamientoByIdHandler _getByIdHandler;
        private readonly IGetEntrenamientosHandler _getAllHandler;
        private readonly IUpdateEntrenamientoHandler _updateHandler;
        private readonly IDarDeBajaEntrenamientoHandler _darDeBajaHandler;
        private readonly IInscribirAlumnoEntrenamientoHandler _inscribirHandler;
        private readonly IRegistrarAsistenciaEntrenamientoHandler _asistenciaHandler;
        private readonly IAsignarEntrenadorParticularHandler _particularHandler;

        public EntrenamientosController(
            ICreateEntrenamientoHandler createHandler,
            IGetEntrenamientoByIdHandler getByIdHandler,
            IGetEntrenamientosHandler getAllHandler,
            IUpdateEntrenamientoHandler updateHandler,
            IDarDeBajaEntrenamientoHandler darDeBajaHandler,
            IInscribirAlumnoEntrenamientoHandler inscribirHandler,
            IRegistrarAsistenciaEntrenamientoHandler asistenciaHandler,
            IAsignarEntrenadorParticularHandler particularHandler)
        {
            _createHandler = createHandler;
            _getByIdHandler = getByIdHandler;
            _getAllHandler = getAllHandler;
            _updateHandler = updateHandler;
            _darDeBajaHandler = darDeBajaHandler;
            _inscribirHandler = inscribirHandler;
            _asistenciaHandler = asistenciaHandler;
            _particularHandler = particularHandler;
        }

        // [RF - 49] Registrar entrenamiento
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CreateEntrenamientoCommand command)
        {
            try
            {
                var entrenamientoCreado = await _createHandler.Handle(command);
                return StatusCode(StatusCodes.Status201Created, entrenamientoCreado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al registrar el entrenamiento.", detalle = ex.Message });
            }
        }

        // [RF - 50] Consultar entrenamiento por ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var entrenamiento = await _getByIdHandler.Handle(new GetEntrenamientoByIdQuery(id));
                if (entrenamiento == null)
                {
                    return NotFound(new { mensaje = $"No se encontró ningún entrenamiento con el ID {id}." });
                }
                return Ok(entrenamiento);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al consultar el entrenamiento.", detalle = ex.Message });
            }
        }

        // [RF - 50] Consultar todos los entrenamientos
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var entrenamientos = await _getAllHandler.Handle(new GetEntrenamientosQuery());
                return Ok(entrenamientos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al consultar los entrenamientos.", detalle = ex.Message });
            }
        }

        // [RF - 51] Modificar entrenamiento
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Modificar(int id, [FromBody] UpdateEntrenamientoCommand command)
        {
            try
            {
                var entrenamientoModificado = await _updateHandler.Handle(id, command);
                if (entrenamientoModificado == null)
                {
                    return NotFound(new { mensaje = $"No se encontró ningún entrenamiento con el ID {id} para modificar." });
                }
                return Ok(entrenamientoModificado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al modificar el entrenamiento.", detalle = ex.Message });
            }
        }

        // [RF - 52] Eliminar entrenamiento (baja lógica)
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DarDeBaja(int id)
        {
            try
            {
                var exito = await _darDeBajaHandler.Handle(id);
                if (!exito)
                {
                    return NotFound(new { mensaje = $"No se encontró ningún entrenamiento con el ID {id} para dar de baja." });
                }
                return Ok(new { mensaje = $"El entrenamiento con ID {id} fue dado de baja correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al dar de baja el entrenamiento.", detalle = ex.Message });
            }
        }

        // [RF - 53] Inscripción de usuarios a entrenamientos
        [HttpPost("{id:int}/inscribir")]
        public async Task<IActionResult> Inscribir(int id, [FromBody] InscribirAlumnoEntrenamientoRequestDto request)
        {
            try
            {
                var command = new InscribirAlumnoEntrenamientoCommand
                {
                    EntrenamientoId = id,
                    Dni = request.Dni
                };

                var resultado = await _inscribirHandler.Handle(command);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al inscribir al alumno en el entrenamiento.", detalle = ex.Message });
            }
        }

        // [RF - 54] Registrar asistencia de usuarios
        [HttpPost("{id:int}/asistencia")]
        public async Task<IActionResult> RegistrarAsistencia(int id, [FromBody] RegistrarAsistenciaEntrenamientoRequestDto request)
        {
            try
            {
                var command = new RegistrarAsistenciaEntrenamientoCommand
                {
                    EntrenamientoId = id,
                    Dni = request.Dni,
                    Presente = request.Presente,
                    Fecha = request.Fecha
                };

                var resultado = await _asistenciaHandler.Handle(command);
                return Ok(resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al registrar la asistencia en el entrenamiento.", detalle = ex.Message });
            }
        }

        // [RF - 54] Asignar entrenador para entrenamiento particular
        [HttpPost("particular")]
        public async Task<IActionResult> AsignarEntrenadorParticular([FromBody] AsignarEntrenadorParticularCommand command)
        {
            try
            {
                var resultado = await _particularHandler.Handle(command);
                return StatusCode(StatusCodes.Status201Created, resultado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al asignar el entrenador para el entrenamiento particular.", detalle = ex.Message });
            }
        }
    }

    public class InscribirAlumnoEntrenamientoRequestDto
    {
        public int Dni { get; set; }
    }

    public class RegistrarAsistenciaEntrenamientoRequestDto
    {
        public int Dni { get; set; }
        public bool Presente { get; set; } = true;
        public DateTime? Fecha { get; set; }
    }
}