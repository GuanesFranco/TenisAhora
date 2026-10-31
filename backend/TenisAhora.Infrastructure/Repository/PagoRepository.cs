using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository;

public class PagoRepository : IPagoRepository
{
    private readonly AppDbContext _context;

    public PagoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Pago> AgregarAsync(Pago pago, CancellationToken ct = default)
    {
        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync(ct);
        return pago;
    }

    public async Task<Pago?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Pagos
            .Include(p => p.Reserva)
                .ThenInclude(r => r!.DetallesStockReserva)
            .Include(p => p.Reserva)
                .ThenInclude(r => r!.Socios)
            .Include(p => p.Inscripcion)
            .Include(p => p.Descuento)
            .Include(p => p.Recibo)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<Pago>> ListarTodosAsync(CancellationToken ct = default)
    {
        return await _context.Pagos
            .Include(p => p.Reserva)
                .ThenInclude(r => r!.DetallesStockReserva)
            .Include(p => p.Reserva)
                .ThenInclude(r => r!.Socios)
            .Include(p => p.Inscripcion)
            .Include(p => p.Descuento)
            .Include(p => p.Recibo)
            .ToListAsync(ct);
    }

    public async Task<List<Pago>> ObtenerPorReservaIdAsync(int reservaId, CancellationToken ct = default)
    {
        return await _context.Pagos
            .Where(p => p.ReservaId == reservaId)
            .ToListAsync(ct);
    }

    public async Task ActualizarAsync(Pago pago, CancellationToken ct = default)
    {
        _context.Pagos.Update(pago);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Descuento?> ObtenerDescuentoPorIdAsync(int descuentoId, CancellationToken ct = default)
    {
        return await _context.Descuentos
            .FirstOrDefaultAsync(d => d.Id == descuentoId, ct);
    }
}
