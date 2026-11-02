using Microsoft.EntityFrameworkCore;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Persona?> ObtenerPorIdAsync(int id)
    {
        return await _context.Personas
            .Include(p => (p as Instructor)!.certificacionDeportiva)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Persona?> ObtenerPorDniAsync(int dni)
    {
        return await _context.Personas
            .Include(p => (p as Instructor)!.certificacionDeportiva)
            .FirstOrDefaultAsync(p => p.Dni == dni);
    }

    public async Task<Persona?> ObtenerPorEmailAsync(string email)
    {
        var emailNormalizado = email.Trim().ToLowerInvariant();
        return await _context.Personas
            .Include(p => (p as Instructor)!.certificacionDeportiva)
            .FirstOrDefaultAsync(p => p.Email.ToLower() == emailNormalizado);
    }

    public async Task<bool> ExisteDniAsync(int dni, int? excluirId = null)
    {
        return await _context.Personas.AnyAsync(p => p.Dni == dni && (!excluirId.HasValue || p.Id != excluirId.Value));
    }

    public async Task<bool> ExisteEmailAsync(string email, int? excluirId = null)
    {
        var emailNormalizado = email.Trim().ToLowerInvariant();
        return await _context.Personas.AnyAsync(p => p.Email.ToLower() == emailNormalizado && (!excluirId.HasValue || p.Id != excluirId.Value));
    }

    public async Task<List<Persona>> ListarTodosAsync(bool soloActivos = true)
    {
        return await _context.Personas
            .Include(p => (p as Instructor)!.certificacionDeportiva)
            .Where(p => !soloActivos || p.Activo)
            .OrderBy(p => p.Apellido)
            .ThenBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task AgregarAsync(Persona persona, CancellationToken cancellationToken = default)
    {
        await _context.Personas.AddAsync(persona, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ActualizarAsync(Persona persona, CancellationToken cancellationToken = default)
    {
        _context.Personas.Update(persona);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Profesor?> ObtenerProfesorPorIdAsync(int id)
    {
        return await _context.Profesores
            .Include(p => p.certificacionDeportiva)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Entrenador?> ObtenerEntrenadorPorIdAsync(int id)
    {
        return await _context.Entrenadores
            .Include(e => e.certificacionDeportiva)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<int?> ObtenerPrimerAdminIdAsync()
    {
        return await _context.Administradores.Select(a => (int?)a.Id).FirstOrDefaultAsync();
    }

    public async Task RegistrarActividadParticularAsync(Actividad actividad, Inscripcion_actividad inscripcion, CancellationToken cancellationToken = default)
    {
        await _context.Actividades.AddAsync(actividad, cancellationToken);
        await _context.InscripcionesActividad.AddAsync(inscripcion, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
