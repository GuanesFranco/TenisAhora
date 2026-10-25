using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Clases.Commands;

namespace TenisAhora.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClasesController : ControllerBase
    {
        private readonly ICreateClaseHandler _createHandler;

        public ClasesController(ICreateClaseHandler createHandler)
        {
            _createHandler = createHandler;
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
    }
}