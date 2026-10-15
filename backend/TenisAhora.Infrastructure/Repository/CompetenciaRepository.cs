using Microsoft.EntityFrameworkCore;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository;

public class CompetenciaRepository : ICompetenciaRepository
{
    private readonly AppDbContext _context;

    public CompetenciaRepository(AppDbContext context)
    {
        _context = context;
    }

    // --- Torneos ---
    public async Task<Torneo?> ObtenerTorneoPorIdAsync(int id)
    {
        return await _context.Torneos
            .Include(t => t.Inscripciones)
                .ThenInclude(i => i.Socio)
            .Include(t => t.Partidos)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<Torneo>> ListarTorneosAsync(bool soloActivos = true)
    {
        return await _context.Torneos
            .Include(t => t.Inscripciones)
                .ThenInclude(i => i.Socio)
            .Include(t => t.Partidos)
            .Where(t => !soloActivos || t.Activo)
            .OrderByDescending(t => t.FechaInicio)
            .ToListAsync();
    }

    public async Task AgregarTorneoAsync(Torneo torneo, CancellationToken cancellationToken = default)
    {
        await _context.Torneos.AddAsync(torneo, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarTorneoAsync(Torneo torneo, CancellationToken cancellationToken = default)
    {
        _context.Torneos.Update(torneo);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // --- Ligas ---
    public async Task<Liga?> ObtenerLigaPorIdAsync(int id)
    {
        return await _context.Ligas
            .Include(l => l.Inscripciones)
                .ThenInclude(i => i.Socio)
            .Include(l => l.Partidos)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<List<Liga>> ListarLigasAsync(bool soloActivos = true)
    {
        return await _context.Ligas
            .Include(l => l.Inscripciones)
                .ThenInclude(i => i.Socio)
            .Include(l => l.Partidos)
            .Where(l => !soloActivos || l.Activo)
            .OrderByDescending(l => l.FechaInicio)
            .ToListAsync();
    }

    public async Task AgregarLigaAsync(Liga liga, CancellationToken cancellationToken = default)
    {
        await _context.Ligas.AddAsync(liga, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarLigaAsync(Liga liga, CancellationToken cancellationToken = default)
    {
        _context.Ligas.Update(liga);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // --- Inscripciones ---
    public async Task<bool> EstaInscriptoAsync(int competenciaId, int socioId)
    {
        return await _context.InscripcionesCompetencia
            .AnyAsync(i => i.CompetenciaId == competenciaId && i.SocioId == socioId);
    }

    public async Task InscribirCompetenciaAsync(Inscripcion_competencia inscripcion, CancellationToken cancellationToken = default)
    {
        await _context.InscripcionesCompetencia.AddAsync(inscripcion, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Inscripcion_competencia>> ObtenerInscripcionesAsync(int competenciaId)
    {
        return await _context.InscripcionesCompetencia
            .Include(i => i.Socio)
            .Where(i => i.CompetenciaId == competenciaId)
            .OrderBy(i => i.NumeroParticipante)
            .ToListAsync();
    }

    // --- Partidos y Fixtures ---
    public async Task<Partido?> ObtenerPartidoPorIdAsync(int partidoId)
    {
        return await _context.Partidos
            .FirstOrDefaultAsync(p => p.Id == partidoId);
    }

    public async Task GuardarPartidosAsync(List<Partido> partidos, CancellationToken cancellationToken = default)
    {
        await _context.Partidos.AddRangeAsync(partidos, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarPartidoAsync(Partido partido, CancellationToken cancellationToken = default)
    {
        _context.Partidos.Update(partido);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Partido>> ObtenerPartidosPorCompetenciaAsync(int competenciaId)
    {
        return await _context.Partidos
            .Where(p => p.CompetenciaId == competenciaId)
            .OrderBy(p => p.fecha)
            .ThenBy(p => p.HoraInicio)
            .ToListAsync();
    }
}
