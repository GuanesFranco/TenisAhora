using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository;

public class CanchaRepository : ICanchaRepository
{
    private readonly AppDbContext _context;

    public CanchaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cancha?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Canchas
            .Include(c => c.TipoDeCancha)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Cancha>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Canchas
            .Include(c => c.TipoDeCancha)
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }
}