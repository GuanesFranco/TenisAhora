using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
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

    public static string ObtenerFotoPorDefecto(Superficie superficie) => superficie switch
    {
        Superficie.Ladrillo => "/courts/ladrillo.jpg?v=3",
        Superficie.Cemento => "/courts/cemento.jpg?v=3",
        Superficie.Pasto => "/courts/pasto.jpg?v=5",
        _ => "/courts/ladrillo.jpg?v=3"
    };

    public async Task<Cancha?> CrearAsync(int tipoDeCanchaId, string? fotoUrl = null)
    {
        var tipoDeCancha = await _context.TiposDeCancha
            .FirstOrDefaultAsync(t => t.Id == tipoDeCanchaId);

        if (tipoDeCancha == null)
        {
            return null;
        }

        var foto = !string.IsNullOrWhiteSpace(fotoUrl)
            ? fotoUrl.Trim()
            : ObtenerFotoPorDefecto(tipoDeCancha.superficie);

        var cancha = new Cancha
        {
            TipoDeCanchaId = tipoDeCanchaId,
            TipoDeCancha = tipoDeCancha,
            FotoUrl = foto
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

    public async Task<Cancha?> ActualizarAsync(int id, int tipoDeCanchaId, string? fotoUrl = null)
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

        var cambioTipo = cancha.TipoDeCanchaId != tipoDeCanchaId;
        cancha.TipoDeCanchaId = tipoDeCanchaId;

        if (!string.IsNullOrWhiteSpace(fotoUrl))
        {
            cancha.FotoUrl = fotoUrl.Trim();
        }
        else if (cambioTipo || string.IsNullOrWhiteSpace(cancha.FotoUrl))
        {
            cancha.FotoUrl = ObtenerFotoPorDefecto(tipoDeCancha.superficie);
        }

        await _context.SaveChangesAsync();

        cancha.TipoDeCancha = tipoDeCancha;
        return cancha;
    }
}
