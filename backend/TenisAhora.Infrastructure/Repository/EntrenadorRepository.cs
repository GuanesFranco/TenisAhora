using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository
{
    public class EntrenadorRepository : IEntrenadorRepository
    {

        private readonly AppDbContext _context;

        public EntrenadorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Entrenador> CrearAsync(Entrenador entrenador)
        {
            _context.Set<Entrenador>().Add(entrenador);
            await _context.SaveChangesAsync();
            return entrenador;
        }


        public async Task<List<Entrenador>> ObtenerTodosAsync()
        {
            return await _context.Set<Entrenador>()
                .ToListAsync();
        }

        public async Task<bool> DarDeBajaAsync(int id)
        {
            var entrenador = await _context.Set<Entrenador>()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entrenador == null)
                return false;

            entrenador.Activo = false;

            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<Entrenador?> ObtenerPorIdParaActualizarAsync(int id)
        {
            return await _context.Set<Entrenador>()
                .Include(e => e.certificacionDeportiva)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }




        public async Task<Entrenador?> ObtenerPorIdAsync(int id)
        {
            return await _context.Set<Entrenador>()
                .Include(e => e.certificacionDeportiva)
                .FirstOrDefaultAsync(e => e.Id == id);
        }


        public async Task<Entrenador?> ObtenerParaImprimirAsync(int id)
        {
            return await _context.Set<Entrenador>()
                .Include(e => e.certificacionDeportiva)
                .FirstOrDefaultAsync(e => e.Id == id);
        }



        public async Task<bool> ReactivarAsync(int id)
        {
            var entrenador = await _context.Set<Entrenador>()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entrenador == null)
                return false;

            entrenador.Activo = true;

            await _context.SaveChangesAsync();

            return true;
        }

    }
}
