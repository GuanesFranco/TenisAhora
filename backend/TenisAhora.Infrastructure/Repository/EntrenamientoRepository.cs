using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository
{
    public class EntrenamientoRepository : IEntrenamientoRepository
    {
        private readonly AppDbContext _context;

        public EntrenamientoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Entrenamiento> CrearAsync(Entrenamiento entrenamiento)
        {
            _context.Entrenamientos.Add(entrenamiento);
            await _context.SaveChangesAsync();
            return entrenamiento;
        }

        public async Task<Entrenamiento?> ObtenerPorIdAsync(int id)
        {
            return await _context.Entrenamientos
                .Where(e => e.Activo)
                .Include(e => e.Entrenador)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Entrenamiento>> ObtenerTodosAsync()
        {
            return await _context.Entrenamientos
                .Where(e => e.Activo)
                .Include(e => e.Entrenador)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task ModificarAsync(Entrenamiento entrenamiento)
        {
            _context.Entrenamientos.Update(entrenamiento);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var entrenamiento = await _context.Entrenamientos.FindAsync(id);
            if (entrenamiento != null)
            {
                entrenamiento.Activo = false; // Baja lógica
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Socio?> ObtenerSocioPorDniAsync(int dni)
        {
            return await _context.Socios
                .FirstOrDefaultAsync(s => s.Dni == dni);
        }

        public async Task<bool> EstaInscriptoAsync(int entrenamientoId, int socioId)
        {
            return await _context.InscripcionesActividad
                .AnyAsync(i => i.ActividadId == entrenamientoId && i.SocioId == socioId && i.estadoInscripcion != EstadoInscripcion.Cancelada);
        }

        public async Task<Inscripcion_actividad> InscribirAlumnoAsync(Inscripcion_actividad inscripcion, Entrenamiento entrenamiento)
        {
            await _context.InscripcionesActividad.AddAsync(inscripcion);
            _context.Entrenamientos.Update(entrenamiento);
            await _context.SaveChangesAsync();
            return inscripcion;
        }

        public async Task<bool> CancelarInscripcionAsync(int entrenamientoId, int dni)
        {
            var socio = await ObtenerSocioPorDniAsync(dni);
            if (socio == null) return false;

            var inscripcion = await _context.InscripcionesActividad
                .FirstOrDefaultAsync(i => i.ActividadId == entrenamientoId && i.SocioId == socio.Id && i.estadoInscripcion != EstadoInscripcion.Cancelada);
            if (inscripcion == null) return false;

            inscripcion.estadoInscripcion = EstadoInscripcion.Cancelada;
            var ent = await _context.Entrenamientos.FindAsync(entrenamientoId);
            if (ent != null && ent.CantidadDeAlumnos > 0)
            {
                ent.CantidadDeAlumnos -= 1;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<int>> ObtenerDnisInscriptosAsync(int entrenamientoId)
        {
            return await _context.InscripcionesActividad
                .Where(i => i.ActividadId == entrenamientoId && i.estadoInscripcion != EstadoInscripcion.Cancelada)
                .Join(_context.Socios, i => i.SocioId, s => s.Id, (i, s) => s.Dni)
                .ToListAsync();
        }

        public async Task<Asistencia?> ObtenerAsistenciaAsync(int entrenamientoId, int socioId, DateTime fecha)
        {
            var inicioDia = fecha.Date;
            var finDia = inicioDia.AddDays(1);

            return await _context.Asistencias
                .FirstOrDefaultAsync(a => a.ActividadId == entrenamientoId && a.SocioId == socioId && a.Fecha >= inicioDia && a.Fecha < finDia);
        }

        public async Task<Asistencia> RegistrarAsistenciaAsync(Asistencia asistencia)
        {
            if (asistencia.Id == 0)
            {
                await _context.Asistencias.AddAsync(asistencia);
            }
            else
            {
                _context.Asistencias.Update(asistencia);
            }

            await _context.SaveChangesAsync();
            return asistencia;
        }

        public async Task<Entrenador?> ObtenerEntrenadorPorIdAsync(int id)
        {
            return await _context.Entrenadores
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Entrenamiento> RegistrarEntrenamientoParticularAsync(Entrenamiento entrenamiento, Inscripcion_actividad inscripcion)
        {
            await _context.Entrenamientos.AddAsync(entrenamiento);
            await _context.InscripcionesActividad.AddAsync(inscripcion);
            await _context.SaveChangesAsync();
            return entrenamiento;
        }
    }
}
