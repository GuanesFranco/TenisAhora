using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class StockReservasRepository : IStockRepository
{
    private readonly AppDbContext _context;

    public StockReservasRepository(AppDbContext context)
    { 
        _context = context;
    }
    public async Task<Stock?> ObtenerPorIdAsync(int id)
    {
        return await _context.Set<Stock>().FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task ActualizarStockAsync(Stock stock)
    {
        _context.Set<Stock>().Update(stock);
        await _context.SaveChangesAsync();
    }
}