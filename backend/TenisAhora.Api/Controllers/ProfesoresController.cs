using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Profesores.Commands;
using TenisAhora.Application.UseCases.Profesores.Queries;
using TenisAhora.Application.UseCases.Profesores.Handlers;




namespace TenisAhora.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfesoresController : ControllerBase
    {
        private readonly ICreateProfesorHandler _handler;
        private readonly IGetProfesorByIdHandler _getHandler;
        private readonly IUpdateProfesorHandler _updateHandler;
        private readonly IGetProfesoresHandler _getTodosHandler;
        private readonly IDarDeBajaProfesorHandler _darDeBajaHandler;
        private readonly IImprimirProfesorHandler _imprimirProfesorHandler;

        public ProfesoresController(ICreateProfesorHandler handler, IGetProfesorByIdHandler getHandler, IUpdateProfesorHandler updateHandler, IGetProfesoresHandler getTodosHandler, IDarDeBajaProfesorHandler darDeBajaHandler, IImprimirProfesorHandler imprimirProfesorHandler)
        {
            _handler = handler;
            _getHandler = getHandler;
            _updateHandler = updateHandler;
            _getTodosHandler = getTodosHandler;
            _darDeBajaHandler = darDeBajaHandler;
            _imprimirProfesorHandler = imprimirProfesorHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] CreateProfesorCommand command)
        {
            var profesorCreado = await _handler.Handle(command);

            return StatusCode(
                StatusCodes.Status201Created,
                profesorCreado);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var query = new GetProfesorByIdQuery(id);

            var profesor = await _getHandler.Handle(query);

            if (profesor is null)
            {
                return NotFound(new
                {
                    mensaje = $"No se encontró un profesor con el ID {id}."
                });
            }

            return Ok(profesor);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Modificar(
        int id,
        [FromBody] UpdateProfesorCommand command)
        {
            var profesorModificado = await _updateHandler.Handle(id, command);

            if (profesorModificado is null)
            {
                return NotFound(new
                {
                    mensaje = $"No se encontró un profesor con el ID {id}."
                });
            }

            return Ok(profesorModificado);
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var query = new GetProfesoresQuery();
            var profesores = await _getTodosHandler.Handle(query);

            return Ok(profesores);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DarDeBaja(int id)
        {
            var resultado = await _darDeBajaHandler.Handle(id);

            if (!resultado)
            {
                return NotFound(new
                {
                    mensaje = $"No se encontró un profesor con el ID {id}."
                });
            }

            return Ok(new
            {
                mensaje = $"El profesor con el ID {id} fue dado de baja correctamente."
            });
        }



        [HttpGet("{id}/imprimir")]
        public async Task<IActionResult> Imprimir(int id)
        {
            var profesor = await _imprimirProfesorHandler.Handle(id);

            if (profesor == null)
                return NotFound();

            return Ok(profesor);
        }


    }
}
