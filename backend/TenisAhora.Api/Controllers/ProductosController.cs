using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TenisAhora.Application.DTOs.Productos;
using TenisAhora.Application.UseCases.Productos;

namespace TenisAhora.Api.Controllers
{
    /// <summary>
    /// Controlador para la Gestión de Productos.
    /// Cumple con las especificaciones [RF - 78], [RF - 79], [RF - 80], [RF - 81] y [RF - 82].
    /// </summary>
    [ApiController]
    [Route("api/productos")]
    public class ProductosController : ControllerBase
    {
        private readonly CrearProductoHandler _crearHandler;
        private readonly ModificarProductoHandler _modificarHandler;
        private readonly ObtenerProductoHandler _obtenerHandler;
        private readonly EliminarProductoHandler _eliminarHandler;
        private readonly ListarProductosHandler _listarHandler;

        public ProductosController(
            CrearProductoHandler crearHandler,
            ModificarProductoHandler modificarHandler,
            ObtenerProductoHandler obtenerHandler,
            EliminarProductoHandler eliminarHandler,
            ListarProductosHandler listarHandler)
        {
            _crearHandler = crearHandler;
            _modificarHandler = modificarHandler;
            _obtenerHandler = obtenerHandler;
            _eliminarHandler = eliminarHandler;
            _listarHandler = listarHandler;
        }

        /// <summary>
        /// [RF - 78] Registra un nuevo producto.
        /// Equipamientos disponibles: StockId 1 = Pelotas de tenis, 2 = Raquetas, 3 = Redes.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearProductoCommand command)
        {
            try
            {
                var resultado = await _crearHandler.EjecutarAsync(command);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error inesperado al registrar el producto.", detalle = ex.Message });
            }
        }

        /// <summary>
        /// [RF - 82] Genera un listado con la totalidad de los productos registrados y disponibles.
        /// Visualiza las marcas y modelos con su precio y cantidad disponible en stock.
        /// </summary>
        [HttpGet]
        [HttpGet("listar")]
        public async Task<ActionResult<List<ProductoResponseDto>>> Listar()
        {
            try
            {
                var productos = await _listarHandler.EjecutarAsync();
                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error inesperado al listar los productos.", detalle = ex.Message });
            }
        }

        /// <summary>
        /// [RF - 80] Consulta un producto por su ID, visualizando su detalle completo, incluyendo stock actual y precio.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductoResponseDto>> ObtenerPorId([FromRoute] int id)
        {
            try
            {
                var producto = await _obtenerHandler.ObtenerPorIdAsync(id);
                return Ok(producto);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error inesperado al consultar el producto.", detalle = ex.Message });
            }
        }

        /// <summary>
        /// [RF - 79] Modifica los datos de un producto (nombre, precio, descripción) identificándolo por su ID.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Modificar([FromRoute] int id, [FromBody] ModificarProductoCommand command)
        {
            try
            {
                var actualizado = await _modificarHandler.EjecutarAsync(id, command);
                return Ok(actualizado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error inesperado al modificar el producto.", detalle = ex.Message });
            }
        }

        /// <summary>
        /// [RF - 81] Elimina o da de baja un producto del catálogo identificándolo por su ID.
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar([FromRoute] int id)
        {
            try
            {
                await _eliminarHandler.EjecutarAsync(id);
                return Ok(new { mensaje = $"El producto con ID {id} fue dado de baja correctamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Ocurrió un error inesperado al eliminar el producto.", detalle = ex.Message });
            }
        }
    }
}
