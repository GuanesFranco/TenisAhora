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

       

       

   



    }
}
