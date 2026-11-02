using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Clases.Commands;
using TenisAhora.Application.UseCases.Clases.Queries;

namespace TenisAhora.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClasesController : ControllerBase
    {
        private readonly ICreateClaseHandler _createHandler;
        private readonly IGetClaseByIdHandler _getByIdHandler;
        private readonly IGetClasesHandler _getAllHandler;
        private readonly IUpdateClaseHandler _updateHandler;
        private readonly IDarDeBajaClaseHandler _darDeBajaHandler;
        private readonly IInscribirAlumnoClaseHandler _inscribirAlumnoHandler;
        private readonly IRegistrarAsistenciaClaseHandler _registrarAsistenciaHandler;
        private readonly IAsignarProfesorParticularHandler _asignarProfesorParticularHandler;
        private readonly IClaseRepository _claseRepository;

        public ClasesController(
            ICreateClaseHandler createHandler,
            IGetClaseByIdHandler getByIdHandler,
            IGetClasesHandler getAllHandler,
            IUpdateClaseHandler updateHandler,
            IDarDeBajaClaseHandler darDeBajaHandler,
            IInscribirAlumnoClaseHandler inscribirAlumnoHandler,
            IRegistrarAsistenciaClaseHandler registrarAsistenciaHandler,
            IAsignarProfesorParticularHandler asignarProfesorParticularHandler,
            IClaseRepository claseRepository)
        {
            _createHandler = createHandler;
            _getByIdHandler = getByIdHandler;
            _getAllHandler = getAllHandler;
            _updateHandler = updateHandler;
            _darDeBajaHandler = darDeBajaHandler;
            _inscribirAlumnoHandler = inscribirAlumnoHandler;
            _registrarAsistenciaHandler = registrarAsistenciaHandler;
            _asignarProfesorParticularHandler = asignarProfesorParticularHandler;
            _claseRepository = claseRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CreateClaseCommand command)
        {
            try
            {
                var claseCreada = await _createHandler.Handle(command);
                return StatusCode(StatusCodes.Status201Created, claseCreada);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al registrar la clase.", detalle = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var query = new GetClaseByIdQuery(id);
            var clase = await _getByIdHandler.Handle(query);

            if (clase == null)
            {
                return NotFound(new { mensaje = $"No se encontró la clase con el ID {id}." });
            }

            return Ok(clase);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var query = new GetClasesQuery();
            var clases = await _getAllHandler.Handle(query);
            return Ok(clases);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Modificar(int id, [FromBody] UpdateClaseCommand command)
        {
            try
            {
                var claseModificada = await _updateHandler.Handle(id, command);

                if (claseModificada == null)
                {
                    return NotFound(new { mensaje = $"No se encontró la clase con el ID {id} para modificar." });
                }

                return Ok(claseModificada);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error al modificar la clase.", detalle = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DarDeBaja(int id)
        {
            var resultado = await _darDeBajaHandler.Handle(id);

            if (!resultado)
            {
                return NotFound(new { mensaje = $"No se encontró la clase con el ID {id}." });
            }

            return Ok(new { mensaje = $"La clase con el ID {id} fue dada de baja correctamente." });
        }

        [HttpPost("{id:int}/inscribir")]
        public async Task<IActionResult> InscribirAlumno(int id, [FromBody] InscribirAlumnoRequestDto request)
        {
            try
            {
                var command = new InscribirAlumnoClaseCommand
                {
                    ClaseId = id,
                    DniUsuario = request.Dni
                };

                var resultado = await _inscribirAlumnoHandler.Handle(command);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al inscribir al alumno.", detalle = ex.Message });
            }
        }

        [HttpDelete("{id:int}/inscribir/{dni:int}")]
        public async Task<IActionResult> CancelarInscripcion(int id, int dni)
        {
            var cancelado = await _claseRepository.CancelarInscripcionAsync(id, dni);
            if (!cancelado)
            {
                return NotFound(new { mensaje = "No se encontró la inscripción para cancelar." });
            }
            return Ok(new { mensaje = "Inscripción cancelada correctamente." });
        }

        [HttpPost("{id:int}/asistencia")]
        public async Task<IActionResult> RegistrarAsistencia(int id, [FromBody] RegistrarAsistenciaRequestDto request)
        {
            try
            {
                var command = new RegistrarAsistenciaClaseCommand
                {
                    ClaseId = id,
                    DniUsuario = request.Dni,
                    Presente = request.Presente,
                    Fecha = request.Fecha
                };

                var resultado = await _registrarAsistenciaHandler.Handle(command);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al registrar la asistencia.", detalle = ex.Message });
            }
        }

        [HttpPost("particular")]
        public async Task<IActionResult> AsignarProfesorParticular([FromBody] AsignarProfesorParticularCommand command)
        {
            try
            {
                var resultado = await _asignarProfesorParticularHandler.Handle(command);
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
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Ocurrió un error inesperado al asignar el profesor para la clase particular.", detalle = ex.Message });
            }
        }
    }

    public class InscribirAlumnoRequestDto
    {
        public int Dni { get; set; }
    }

    public class RegistrarAsistenciaRequestDto
    {
        public int Dni { get; set; }
        public bool Presente { get; set; } = true;
        public DateTime? Fecha { get; set; }
    }
}
