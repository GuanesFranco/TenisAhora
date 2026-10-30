using System;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Productos;
using TenisAhora.Application.Interfaces;

namespace TenisAhora.Application.UseCases.Productos
{
    public class ObtenerProductoHandler
    {
        private readonly IProductoRepository _productoRepository;

        public ObtenerProductoHandler(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task<ProductoResponseDto> ObtenerPorIdAsync(int id)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);
            if (producto == null)
                throw new KeyNotFoundException($"No se encontró ningún producto activo con el ID {id}.");

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

        public async Task<ProductoResponseDto> ObtenerPorNombreAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre a consultar no puede estar vacío.");

            var producto = await _productoRepository.ObtenerPorNombreAsync(nombre.Trim());
            if (producto == null)
                throw new KeyNotFoundException($"No se encontró ningún producto activo con el nombre '{nombre}'.");

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
