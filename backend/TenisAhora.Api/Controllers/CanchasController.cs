using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Canchas.Commands;
using TenisAhora.Application.UseCases.Canchas.Queries;

namespace TenisAhora.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CanchasController : ControllerBase
    {
        private readonly ICreateCanchaHandler _handler;
        private readonly IDeleteCanchaHandler _deleteHandler;
        private readonly IGetCanchaByIdHandler _getByIdHandler;
        private readonly IGetAllCanchasHandler _getAllHandler;
        private readonly IUpdateCanchaHandler _updateHandler;

        public CanchasController(ICreateCanchaHandler handler, IDeleteCanchaHandler deleteHandler, IGetCanchaByIdHandler getByIdHandler, IGetAllCanchasHandler getAllHandler, IUpdateCanchaHandler updateHandler)
        {
            _handler = handler;
            _deleteHandler = deleteHandler;
            _getByIdHandler = getByIdHandler;
            _getAllHandler = getAllHandler;
            _updateHandler = updateHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CreateCanchaCommand command)
        {
            var canchaCreada = await _handler.Handle(command);

            if (canchaCreada == null)
            {
                return BadRequest(
                    "El tipo de cancha indicado no existe.");
            }

            return StatusCode(
                StatusCodes.Status201Created,
                canchaCreada);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var eliminada = await _deleteHandler.Handle(
                    new DeleteCanchaCommand { Id = id });

                if (!eliminada)
                    return NotFound("No se encontró la cancha.");

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var cancha = await _getByIdHandler.Handle(
                new GetCanchaByIdQuery { Id = id });

            if (cancha == null)
                return NotFound("No se encontró la cancha.");

            return Ok(cancha);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodas()
        {
            var canchas = await _getAllHandler.Handle(
                new GetAllCanchasQuery());

            return Ok(canchas);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] UpdateCanchaCommand command)
        {
            command.Id = id;

            try
            {
                var cancha = await _updateHandler.Handle(command);

                if (cancha == null)
                    return NotFound("No se encontró la cancha.");

                return Ok(cancha);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
