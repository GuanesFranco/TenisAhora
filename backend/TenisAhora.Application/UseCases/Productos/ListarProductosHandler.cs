using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Productos;
using TenisAhora.Application.Interfaces;

namespace TenisAhora.Application.UseCases.Productos
{
    public class ListarProductosHandler
    {
        private readonly IProductoRepository _productoRepository;

        public ListarProductosHandler(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task<List<ProductoResponseDto>> EjecutarAsync()
        {
            var productos = await _productoRepository.ListarTodosAsync(soloActivos: true);

            return productos.Select(p => new ProductoResponseDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                StockId = p.StockId,
                TipoEquipamiento = p.Stock?.tipo_elemento ?? string.Empty,
                StockActual = p.Stock?.cantidad_disponible ?? 0,
                Activo = p.Activo
            }).ToList();
        }
    }
}
