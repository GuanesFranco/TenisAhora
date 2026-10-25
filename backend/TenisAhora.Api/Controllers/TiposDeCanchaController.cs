using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.TiposDeCancha.Commands;
using TenisAhora.Application.UseCases.TiposDeCancha.Queries;

namespace TenisAhora.Api.Controllers
{
    [ApiController]
    [Route("api/tipos-de-cancha")]
    public class TiposDeCanchaController : ControllerBase
    {
        private readonly ICreateTipoDeCanchaHandler _createHandler;
        private readonly IUpdateTipoDeCanchaHandler _updateHandler;
        private readonly IGetTipoDeCanchaByIdHandler _getByIdHandler;
        private readonly IGetTiposDeCanchaHandler _getAllHandler;

        public TiposDeCanchaController(
            ICreateTipoDeCanchaHandler createHandler,
            IUpdateTipoDeCanchaHandler updateHandler,
            IGetTipoDeCanchaByIdHandler getByIdHandler,
            IGetTiposDeCanchaHandler getAllHandler)
        {
            _createHandler = createHandler;
            _updateHandler = updateHandler;
            _getByIdHandler = getByIdHandler;
            _getAllHandler = getAllHandler;
        }

        /// <summary>
        /// [RF-08] Registrar un nuevo tipo de cancha (Ladrillo, Cemento, Pasto, superficie, capacidad, precio, características).
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TipoDeCanchaResponseDto>> Crear([FromBody] CreateTipoDeCanchaCommand command)
        {
            var creado = await _createHandler.Handle(command);
            return StatusCode(StatusCodes.Status201Created, creado);
        }

        /// <summary>
        /// [RF-09] Modificar un tipo de cancha existente por su ID.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<TipoDeCanchaResponseDto>> Modificar(int id, [FromBody] UpdateTipoDeCanchaCommand command)
        {
            var modificado = await _updateHandler.Handle(id, command);

            if (modificado is null)
            {
                return NotFound(new
                {
                    mensaje = $"No se encontró un tipo de cancha con el ID {id}."
                });
            }

            return Ok(modificado);
        }

        /// <summary>
        /// [RF-10] Consultar los datos de un tipo de cancha mediante su ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TipoDeCanchaResponseDto>> ObtenerPorId(int id)
        {
            var query = new GetTipoDeCanchaByIdQuery(id);
            var tipoDeCancha = await _getByIdHandler.Handle(query);

            if (tipoDeCancha is null)
            {
                return NotFound(new
                {
                    mensaje = $"No se encontró un tipo de cancha con el ID {id}."
                });
            }

            return Ok(tipoDeCancha);
        }

        /// <summary>
        /// [RF-10] Consultar todos los tipos de cancha registrados.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<TipoDeCanchaResponseDto>>> ObtenerTodos()
        {
            var query = new GetTiposDeCanchaQuery();
            var lista = await _getAllHandler.Handle(query);
            return Ok(lista);
        }
    }
}

