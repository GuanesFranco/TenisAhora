using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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

        public ClasesController(
            ICreateClaseHandler createHandler,
            IGetClaseByIdHandler getByIdHandler,
            IGetClasesHandler getAllHandler,
            IUpdateClaseHandler updateHandler,
            IDarDeBajaClaseHandler darDeBajaHandler)
        {
            _createHandler = createHandler;
            _getByIdHandler = getByIdHandler;
            _getAllHandler = getAllHandler;
            _updateHandler = updateHandler;
            _darDeBajaHandler = darDeBajaHandler;
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
    }
}