using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository
{
    public class TipoDeCanchaRepository : ITipoDeCanchaRepository
    {
        private readonly AppDbContext _context;

        public TipoDeCanchaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TipoDeCancha> CrearAsync(TipoDeCancha tipoDeCancha)
        {
            await _context.TiposDeCancha.AddAsync(tipoDeCancha);
            await _context.SaveChangesAsync();
            return tipoDeCancha;
        }

        public async Task<TipoDeCancha?> ObtenerPorIdAsync(int id)
        {
            return await _context.TiposDeCancha
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TipoDeCancha?> ObtenerPorIdParaActualizarAsync(int id)
        {
            return await _context.TiposDeCancha
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<TipoDeCancha>> ObtenerTodosAsync()
        {
            return await _context.TiposDeCancha
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task ActualizarAsync(TipoDeCancha tipoDeCancha)
        {
            _context.TiposDeCancha.Update(tipoDeCancha);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> TieneCanchasAsociadasAsync(int id)
        {
            return await _context.Canchas
                .AnyAsync(c => c.TipoDeCanchaId == id);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var tipoDeCancha = await _context.TiposDeCancha
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tipoDeCancha is null)
            {
                return false;
            }

            _context.TiposDeCancha.Remove(tipoDeCancha);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

