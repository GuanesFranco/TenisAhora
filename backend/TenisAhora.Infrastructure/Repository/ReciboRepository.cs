using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository;

public class ReciboRepository : IReciboRepository
{
    private readonly AppDbContext _context;

    public ReciboRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Recibo> AgregarAsync(Recibo recibo, CancellationToken ct = default)
    {
        _context.Recibos.Add(recibo);
        await _context.SaveChangesAsync(ct);
        return recibo;
    }

    public async Task<Recibo?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Recibos
            .Include(r => r.Pago)
                .ThenInclude(p => p.Reserva)
                    .ThenInclude(res => res!.Socios)
            .Include(r => r.Pago)
                .ThenInclude(p => p.Inscripcion)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<Recibo?> ObtenerPorPagoIdAsync(int pagoId, CancellationToken ct = default)
    {
        return await _context.Recibos
            .Include(r => r.Pago)
                .ThenInclude(p => p.Reserva)
                    .ThenInclude(res => res!.Socios)
            .Include(r => r.Pago)
                .ThenInclude(p => p.Inscripcion)
            .FirstOrDefaultAsync(r => r.PagoId == pagoId, ct);
    }

    public async Task<List<Recibo>> ListarTodosAsync(CancellationToken ct = default)
    {
        return await _context.Recibos
            .Include(r => r.Pago)
                .ThenInclude(p => p.Reserva)
                    .ThenInclude(res => res!.Socios)
            .Include(r => r.Pago)
                .ThenInclude(p => p.Inscripcion)
            .ToListAsync(ct);
    }

    public async Task ActualizarAsync(Recibo recibo, CancellationToken ct = default)
    {
        _context.Recibos.Update(recibo);
        await _context.SaveChangesAsync(ct);
    }
}
