using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces
{
    public interface IProductoRepository
    {
        Task<Producto?> ObtenerPorIdAsync(int id, bool incluirInactivos = false);
        Task<Producto?> ObtenerPorNombreAsync(string nombre, bool incluirInactivos = false);
        Task<List<Producto>> ListarTodosAsync(bool soloActivos = true);
        Task<Producto> CrearAsync(Producto producto);
        Task ModificarAsync(Producto producto);
        Task<Stock?> ObtenerStockPorIdAsync(int stockId);
        Task GuardarCambiosAsync();
    }
}
