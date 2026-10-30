using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TenisAhora.Infrastructure.Repository;

public class CanchaRepository : ICanchaRepository
{
    private readonly AppDbContext _context;

    public CanchaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cancha?> CrearAsync(int tipoDeCanchaId)
    {
        var tipoDeCancha = await _context.TiposDeCancha
            .FirstOrDefaultAsync(t => t.Id == tipoDeCanchaId);

        if (tipoDeCancha == null)
        {
            return null;
        }

        var cancha = new Cancha
        {
            TipoDeCanchaId = tipoDeCanchaId,
            TipoDeCancha = tipoDeCancha
        };

        _context.Canchas.Add(cancha);

        await _context.SaveChangesAsync();

        return cancha;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var cancha = await _context.Canchas
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cancha == null)
            return false;

        var tieneReservas = await _context.Reservas
            .AnyAsync(r => r.CanchaId == id);

        var tienePartidos = await _context.Partidos
            .AnyAsync(p => p.CanchaId == id);

        if (tieneReservas || tienePartidos)
        {
            throw new InvalidOperationException(
                "No se puede eliminar una cancha que tiene reservas o partidos asociados.");
        }

        _context.Canchas.Remove(cancha);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Cancha?> ObtenerPorIdAsync(int id)
    {
        return await _context.Canchas
            .Include(c => c.TipoDeCancha)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Cancha>> ObtenerTodasAsync()
    {
        return await _context.Canchas
            .Include(c => c.TipoDeCancha)
            .ToListAsync();
    }

    public async Task<Cancha?> ActualizarAsync(int id, int tipoDeCanchaId)
    {
        var cancha = await _context.Canchas
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cancha == null)
            return null;

        var tipoDeCancha = await _context.TiposDeCancha
            .FirstOrDefaultAsync(t => t.Id == tipoDeCanchaId);

        if (tipoDeCancha == null)
        {
            throw new ArgumentException(
                "No existe el tipo de cancha indicado.");
        }

        cancha.TipoDeCanchaId = tipoDeCanchaId;

        await _context.SaveChangesAsync();

        cancha.TipoDeCancha = tipoDeCancha;

        return cancha;
    }
}

