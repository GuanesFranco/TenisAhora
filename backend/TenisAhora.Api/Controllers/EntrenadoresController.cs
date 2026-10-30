using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Entrenadores.Commands;


namespace TenisAhora.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntrenadoresController : ControllerBase
    {
        private readonly ICreateEntrenadorHandler _createEntrenadorHandler;
        private readonly IGetEntrenadoresHandler _getEntrenadoresHandler;
        private readonly IDeleteEntrenadorHandler _deleteEntrenadorHandler;
        private readonly IUpdateEntrenadorHandler _updateEntrenadorHandler;
        private readonly IGetEntrenadorPorIdHandler _getEntrenadorPorIdHandler;
        private readonly IImprimirEntrenadorHandler _imprimirEntrenadorHandler;
        private readonly IReactivarEntrenadorHandler _reactivarEntrenadorHandler;

        public EntrenadoresController( ICreateEntrenadorHandler createEntrenadorHandler, IGetEntrenadoresHandler getEntrenadoresHandler, IDeleteEntrenadorHandler deleteEntrenadorHandler, IUpdateEntrenadorHandler updateEntrenadorHandler, IGetEntrenadorPorIdHandler getEntrenadorPorIdHandler, IImprimirEntrenadorHandler imprimirEntrenadorHandler, IReactivarEntrenadorHandler reactivarEntrenadorHandler)
        {
            _createEntrenadorHandler = createEntrenadorHandler;
            _getEntrenadoresHandler = getEntrenadoresHandler;
            _deleteEntrenadorHandler = deleteEntrenadorHandler;
            _updateEntrenadorHandler = updateEntrenadorHandler;
            _getEntrenadorPorIdHandler = getEntrenadorPorIdHandler;
            _imprimirEntrenadorHandler = imprimirEntrenadorHandler;
            _reactivarEntrenadorHandler = reactivarEntrenadorHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Crear( [FromBody] CreateEntrenadorCommand command)
        {
            var entrenador = await _createEntrenadorHandler.Handle(command);

            return CreatedAtAction(nameof(Crear), new { id = entrenador.Id }, entrenador);
        }


        [HttpGet]
        public async Task<ActionResult<List<EntrenadorResponseDto>>> ObtenerTodos()
        {
            var entrenadores = await _getEntrenadoresHandler.Handle();
            return Ok(entrenadores);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DarDeBaja(int id)
        {
            var resultado = await _deleteEntrenadorHandler.Handle(id);

            if (!resultado)
                return NotFound();

            return NoContent();
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<EntrenadorResponseDto>> Actualizar(int id, [FromBody] UpdateEntrenadorCommand command)
        {
            var entrenadorActualizado =
                await _updateEntrenadorHandler.Handle(id, command);

            if (entrenadorActualizado is null)
                return NotFound();

            return Ok(entrenadorActualizado);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var entrenador = await _getEntrenadorPorIdHandler.Handle(id);

            if (entrenador == null)
                return NotFound();

            return Ok(entrenador);
        }


        [HttpGet("{id}/imprimir")]
        public async Task<IActionResult> Imprimir(int id)
        {
            var entrenador =
                await _imprimirEntrenadorHandler.Handle(id);

            if (entrenador == null)
                return NotFound();

            return Ok(entrenador);
        }


        [HttpPut("{id}/activar")]
        public async Task<IActionResult> Reactivar(int id)
        {
            var resultado = await _reactivarEntrenadorHandler.Handle(id);

            if (!resultado)
                return NotFound();

            return NoContent();
        }


    }
}
