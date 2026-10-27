using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
                .Include(e => e.Entrenador)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Entrenamiento>> ObtenerTodosAsync()
        {
            return await _context.Entrenamientos
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