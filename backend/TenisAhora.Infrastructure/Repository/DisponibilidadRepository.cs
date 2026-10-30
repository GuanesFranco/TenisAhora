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

public class DisponibilidadRepository : IDisponibilidadRepository
{
    private readonly AppDbContext _context;

    public DisponibilidadRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Disponibilidad?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Disponibilidades
            .Include(d => d.Cancha)
                .ThenInclude(c => c.TipoDeCancha)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Disponibilidad>> ObtenerPorCanchaAsync(int canchaId, CancellationToken cancellationToken = default)
    {
        return await _context.Disponibilidades
            .Include(d => d.Cancha)
                .ThenInclude(c => c.TipoDeCancha)
            .AsNoTracking()
            .Where(d => d.CanchaId == canchaId)
            .OrderBy(d => d.FechaHoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Disponibilidad>> ObtenerPorCanchaYFiltrosAsync(int canchaId, DateTime? fechaDesde, DateTime? fechaHasta, EstadoDisponibilidad? estado, CancellationToken cancellationToken = default)
    {
        var query = _context.Disponibilidades
            .Include(d => d.Cancha)
                .ThenInclude(c => c.TipoDeCancha)
            .AsNoTracking()
            .Where(d => d.CanchaId == canchaId);

        if (fechaDesde.HasValue)
        {
            query = query.Where(d => d.FechaHoraFin >= fechaDesde.Value);
        }

        if (fechaHasta.HasValue)
        {
            query = query.Where(d => d.FechaHoraInicio <= fechaHasta.Value);
        }

        if (estado.HasValue)
        {
            query = query.Where(d => d.estado == estado.Value);
        }

        return await query.OrderBy(d => d.FechaHoraInicio).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteSolapamientoAsync(int canchaId, DateTime inicio, DateTime fin, CancellationToken cancellationToken = default)
    {
        return await ExisteSolapamientoAsync(canchaId, inicio, fin, 0, cancellationToken);
    }

    public async Task<bool> ExisteSolapamientoAsync(int canchaId, DateTime inicio, DateTime fin, int excluirDisponibilidadId, CancellationToken cancellationToken = default)
    {
        return await _context.Disponibilidades.AnyAsync(d =>
            d.CanchaId == canchaId &&
            d.Id != excluirDisponibilidadId &&
            inicio < d.FechaHoraFin && fin > d.FechaHoraInicio,
            cancellationToken);
    }

    public async Task CrearAsync(Disponibilidad disponibilidad, CancellationToken cancellationToken = default)
    {
        await _context.Disponibilidades.AddAsync(disponibilidad, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Disponibilidad disponibilidad, CancellationToken cancellationToken = default)
    {
        _context.Disponibilidades.Update(disponibilidad);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var disponibilidad = await _context.Disponibilidades.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (disponibilidad is null)
        {
            return false;
        }

        _context.Disponibilidades.Remove(disponibilidad);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}