using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure
{
    public class StockReservasRepository : IStockRepository
    {
        private readonly AppDbContext _context;

        public StockReservasRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Stock?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Stock>().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<List<Stock>> ListarTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<Stock>().ToListAsync(cancellationToken);
        }

        public async Task<bool> ValidarStockDisponibleAsync(int equipamientoId, int cantidad, CancellationToken cancellationToken = default)
        {
            var item = await _context.Set<Stock>().FirstOrDefaultAsync(s => s.Id == equipamientoId, cancellationToken);
            return item != null && item.cantidad_disponible >= cantidad;
        }

        public async Task DescontarStockAsync(int equipamientoId, int cantidad, CancellationToken cancellationToken = default)
        {
            var item = await _context.Set<Stock>().FirstOrDefaultAsync(s => s.Id == equipamientoId, cancellationToken);
            if (item != null)
            {
                item.cantidad_disponible -= cantidad;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task IncrementarStockAsync(int equipamientoId, int cantidad, CancellationToken cancellationToken = default)
        {
            var item = await _context.Set<Stock>().FirstOrDefaultAsync(s => s.Id == equipamientoId, cancellationToken);
            if (item != null)
            {
                item.cantidad_disponible += cantidad;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<Stock> AgregarAsync(Stock stock, CancellationToken cancellationToken = default)
        {
            _context.Stocks.Add(stock);
            await _context.SaveChangesAsync(cancellationToken);
            return stock;
        }

        public async Task ActualizarAsync(Stock stock, CancellationToken cancellationToken = default)
        {
            _context.Stocks.Update(stock);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task EliminarAsync(Stock stock, CancellationToken cancellationToken = default)
        {
            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> TieneReservasAsociadasAsync(int stockId, CancellationToken cancellationToken = default)
        {
            return await _context.DetallesStockReserva.AnyAsync(d => d.StockId == stockId, cancellationToken);
        }
    }
}
