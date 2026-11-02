using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly AppDbContext _context;

        public ProductoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id, bool incluirInactivos = false)
        {
            var query = _context.Productos.Include(p => p.Stock).AsQueryable();

            if (!incluirInactivos)
            {
                query = query.Where(p => p.Activo);
            }

            return await query.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Producto?> ObtenerPorNombreAsync(string nombre, bool incluirInactivos = false)
        {
            var query = _context.Productos.Include(p => p.Stock).AsQueryable();

            if (!incluirInactivos)
            {
                query = query.Where(p => p.Activo);
            }

            return await query.FirstOrDefaultAsync(p => p.Nombre.ToLower() == nombre.ToLower());
        }

        public async Task<List<Producto>> ListarTodosAsync(bool soloActivos = true)
        {
            var query = _context.Productos.Include(p => p.Stock).AsQueryable();

            if (soloActivos)
            {
                query = query.Where(p => p.Activo);
            }

            // Ordena primero por StockId (1: Pelotas, 2: Raquetas, 3: Redes) y luego alfabéticamente
            return await query.OrderBy(p => p.StockId).ThenBy(p => p.Nombre).ToListAsync();
        }

        public async Task<Producto> CrearAsync(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            // Carga explícita de los datos del Stock (tipo de equipamiento y cantidad disponible)
            await _context.Entry(producto).Reference(p => p.Stock).LoadAsync();

            return producto;
        }

        public async Task ModificarAsync(Producto producto)
        {
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
            await _context.Entry(producto).Reference(p => p.Stock).LoadAsync();
        }

        public async Task<Stock?> ObtenerStockPorIdAsync(int stockId)
        {
            return await _context.Stocks.FirstOrDefaultAsync(s => s.Id == stockId);
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
