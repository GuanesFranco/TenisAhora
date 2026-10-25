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

        public ClasesController(
            ICreateClaseHandler createHandler,
            IGetClaseByIdHandler getByIdHandler,
            IGetClasesHandler getAllHandler)
        {
            _createHandler = createHandler;
            _getByIdHandler = getByIdHandler;
            _getAllHandler = getAllHandler;
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
    }
}