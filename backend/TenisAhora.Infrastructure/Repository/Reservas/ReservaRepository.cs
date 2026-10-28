using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository;

public class ReservaRepository : IReservaRepository
{
    private readonly AppDbContext _context;

    public ReservaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Reserva?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservas
            .Include(r => r.Socios)
            .Include(r => r.Cancha)
            .Include(r => r.Pagos)
            .Include(r => r.DetallesStockReserva)
                .ThenInclude(d => d.Stock)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Reserva>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reservas
            .Include(r => r.Socios)
            .Include(r => r.Cancha)
            .Include(r => r.Pagos)
            .Include(r => r.DetallesStockReserva)
                .ThenInclude(d => d.Stock)
            .OrderByDescending(r => r.fecha_reserva)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteSuperposicionAsync(int canchaId, DateTime inicio, DateTime fin, CancellationToken cancellationToken = default)
    {
        return await ExisteSuperposicionAsync(canchaId, inicio, fin, null, cancellationToken);
    }

    public async Task<bool> ExisteSuperposicionAsync(int canchaId, DateTime inicio, DateTime fin, int? excluirReservaId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservas.AnyAsync(r =>
            r.CanchaId == canchaId &&
            r.estado != EstadoReserva.Cancelada &&
            (!excluirReservaId.HasValue || r.Id != excluirReservaId.Value) &&
            inicio < r.HoraFin && fin > r.HoraInicio,
            cancellationToken);
    }

    public async Task AgregarReservaAsync(Reserva reserva, CancellationToken cancellationToken = default)
    {
        await _context.Reservas.AddAsync(reserva, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarReservaAsync(Reserva reserva, CancellationToken cancellationToken = default)
    {
        _context.Reservas.Update(reserva);
        await _context.SaveChangesAsync(cancellationToken);
    }
}