using System;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;

namespace TenisAhora.Application.UseCases.Productos
{
    public class EliminarProductoHandler
    {
        private readonly IProductoRepository _productoRepository;

        public EliminarProductoHandler(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task EjecutarAsync(int id)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);
            if (producto == null)
                throw new KeyNotFoundException($"No se encontró ningún producto activo con el ID {id}.");

            // Baja lógica
            producto.Activo = false;
            await _productoRepository.ModificarAsync(producto);
        }
    }
}
