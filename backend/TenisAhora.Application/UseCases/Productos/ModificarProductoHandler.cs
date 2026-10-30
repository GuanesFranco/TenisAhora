using System;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Productos;
using TenisAhora.Application.Interfaces;

namespace TenisAhora.Application.UseCases.Productos
{
    public class ModificarProductoHandler
    {
        private readonly IProductoRepository _productoRepository;

        public ModificarProductoHandler(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task<ProductoResponseDto> EjecutarAsync(int id, ModificarProductoCommand command)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);
            if (producto == null)
                throw new KeyNotFoundException($"No se encontró ningún producto activo con el ID {id}.");

            if (string.IsNullOrWhiteSpace(command.Nombre))
                throw new ArgumentException("El nombre del producto no puede estar vacío.");

            if (command.Precio <= 0)
                throw new ArgumentException("El precio de venta debe ser mayor a 0.");

            // Si cambia el nombre, validar que no choque con otro producto activo distinto
            var duplicado = await _productoRepository.ObtenerPorNombreAsync(command.Nombre);
            if (duplicado != null && duplicado.Id != id)
                throw new InvalidOperationException($"Ya existe otro producto activo con el nombre '{command.Nombre}'.");

            if (command.StockId.HasValue)
            {
                if (command.StockId.Value < 1 || command.StockId.Value > 3)
                    throw new ArgumentException("Solo se permiten los equipamientos oficiales del club: 1 (Pelotas de tenis), 2 (Raquetas) o 3 (Redes).");

                var stock = await _productoRepository.ObtenerStockPorIdAsync(command.StockId.Value);
                if (stock == null)
                    throw new ArgumentException($"El StockId {command.StockId.Value} no existe. Equipamientos disponibles: 1 (Pelotas de tenis), 2 (Raquetas), 3 (Redes).");

                producto.StockId = command.StockId.Value;
            }

            producto.Nombre = command.Nombre.Trim();
            producto.Descripcion = command.Descripcion.Trim();
            producto.Precio = command.Precio;

            await _productoRepository.ModificarAsync(producto);

            return new ProductoResponseDto
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                StockId = producto.StockId,
                TipoEquipamiento = producto.Stock?.tipo_elemento ?? string.Empty,
                StockActual = producto.Stock?.cantidad_disponible ?? 0,
                Activo = producto.Activo
            };
        }
    }
}
