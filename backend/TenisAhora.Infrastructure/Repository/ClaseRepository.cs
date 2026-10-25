using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository
{
    public class ClaseRepository : IClaseRepository
    {
        private readonly AppDbContext _context;

        public ClaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Clase> CrearAsync(Clase clase)
        {
            _context.Set<Clase>().Add(clase);
            await _context.SaveChangesAsync();
            return clase;
        }

        public async Task<Clase?> ObtenerPorIdAsync(int id)
        {
            return await _context.Set<Clase>()
                .Include(c => c.Profesor)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Clase>> ObtenerTodasAsync()
        {
            return await _context.Set<Clase>()
                .Include(c => c.Profesor)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task ModificarAsync(Clase clase)
        {
            _context.Set<Clase>().Update(clase);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var clase = await _context.Set<Clase>().FindAsync(id);
            if (clase != null)
            {
                clase.Activo = false; // Baja lógica
                await _context.SaveChangesAsync();
            }
        }
    }
}