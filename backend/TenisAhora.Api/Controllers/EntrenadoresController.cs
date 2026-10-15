using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Entrenadores.Commands;


namespace TenisAhora.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntrenadoresController : ControllerBase
    {
        private readonly ICreateEntrenadorHandler _createEntrenadorHandler;

        public EntrenadoresController(
            ICreateEntrenadorHandler createEntrenadorHandler)
        {
            _createEntrenadorHandler = createEntrenadorHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Crear( [FromBody] CreateEntrenadorCommand command)
        {
            var entrenador = await _createEntrenadorHandler.Handle(command);

            return CreatedAtAction(nameof(Crear), new { id = entrenador.Id }, entrenador);
        }



    }
}
