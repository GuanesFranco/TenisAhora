using System;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Productos;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.UseCases.Productos
{
    public class CrearProductoHandler
    {
        private readonly IProductoRepository _productoRepository;

        public CrearProductoHandler(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task<ProductoResponseDto> EjecutarAsync(CrearProductoCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Nombre))
                throw new ArgumentException("El nombre del producto no puede estar vacío.");

            if (command.Precio <= 0)
                throw new ArgumentException("El precio de venta debe ser mayor a 0.");

            if (command.StockId < 1 || command.StockId > 3)
                throw new ArgumentException("Solo se permiten los equipamientos oficiales del club: 1 (Pelotas de tenis), 2 (Raquetas) o 3 (Redes).");

            // Validar que el equipamiento en stock exista
            var stock = await _productoRepository.ObtenerStockPorIdAsync(command.StockId);
            if (stock == null)
                throw new ArgumentException($"El StockId {command.StockId} no existe. Equipamientos disponibles en el club: 1 (Pelotas de tenis), 2 (Raquetas), 3 (Redes).");

            var existente = await _productoRepository.ObtenerPorNombreAsync(command.Nombre);
            if (existente != null)
                throw new InvalidOperationException($"Ya existe un producto activo con el nombre '{command.Nombre}'.");

            var producto = new Producto
            {
                Nombre = command.Nombre.Trim(),
                Descripcion = command.Descripcion.Trim(),
                Precio = command.Precio,
                StockId = stock.Id,
                Activo = true
            };

            var creado = await _productoRepository.CrearAsync(producto);

            return new ProductoResponseDto
            {
                Id = creado.Id,
                Nombre = creado.Nombre,
                Descripcion = creado.Descripcion,
                Precio = creado.Precio,
                StockId = creado.StockId,
                TipoEquipamiento = creado.Stock?.tipo_elemento ?? stock.tipo_elemento,
                StockActual = creado.Stock?.cantidad_disponible ?? stock.cantidad_disponible,
                Activo = creado.Activo
            };
        }
    }
}
