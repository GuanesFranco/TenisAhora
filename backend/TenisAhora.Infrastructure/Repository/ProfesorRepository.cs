using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository
{
    public class ProfesorRepository : IProfesorRepository
    {
        private readonly AppDbContext _context;

        public ProfesorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Profesor> CrearAsync(Profesor profesor)
        {
            _context.Set<Profesor>().Add(profesor);

            await _context.SaveChangesAsync();

            return profesor;
        }



        public async Task<Profesor?> ObtenerPorIdAsync(int id)
        {
            return await _context.Set<Profesor>()
                .Include(p => p.certificacionDeportiva)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }



        
        public async Task<Profesor?> ObtenerPorIdParaActualizarAsync(
            int id)
        {
            return await _context.Set<Profesor>()
                .Include(p => p.certificacionDeportiva)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

       
        public async Task GuardarCambiosAsync()
        {
            await _context.SaveChangesAsync();
        }


        public async Task<List<Profesor>> ObtenerTodosAsync()
        {
            return await _context.Set<Profesor>()
                .Include(p => p.certificacionDeportiva)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<bool> DarDeBajaAsync(int id)
        {
            var profesor = await _context.Set<Profesor>()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profesor is null)
                return false;

            profesor.Activo = false;

            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<Profesor?> ObtenerParaImprimirAsync(int id)
        {
            return await _context.Set<Profesor>()
                .Include(p => p.certificacionDeportiva)
                .FirstOrDefaultAsync(p => p.Id == id);
        }



        public async Task<bool> ReactivarAsync(int id)
        {
            var profesor = await _context.Set<Profesor>()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profesor == null)
                return false;

            profesor.Activo = true;

            await _context.SaveChangesAsync();

            return true;
        }

    }
}
