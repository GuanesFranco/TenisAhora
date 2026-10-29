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
    public class ClaseRepository : IClaseRepository
    {
        private readonly AppDbContext _context;

        public ClaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Clase> CrearAsync(Clase clase)
        {
            _context.Clases.Add(clase);
            await _context.SaveChangesAsync();
            return clase;
        }

        public async Task<Clase?> ObtenerPorIdAsync(int id)
        {
            return await _context.Clases
                .Include(c => c.Profesor)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Clase>> ObtenerTodasAsync()
        {
            return await _context.Clases
                .Include(c => c.Profesor)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task ModificarAsync(Clase clase)
        {
            _context.Clases.Update(clase);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var clase = await _context.Clases.FindAsync(id);
            if (clase != null)
            {
                clase.Activo = false; // Baja lógica
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Socio?> ObtenerSocioPorDniAsync(int dni)
        {
            return await _context.Socios
                .FirstOrDefaultAsync(s => s.Dni == dni);
        }

        public async Task<bool> EstaInscriptoAsync(int claseId, int socioId)
        {
            return await _context.InscripcionesActividad
                .AnyAsync(i => i.ActividadId == claseId && i.SocioId == socioId && i.estadoInscripcion != EstadoInscripcion.Cancelada);
        }

        public async Task<Inscripcion_actividad> InscribirAlumnoAsync(Inscripcion_actividad inscripcion, Clase clase)
        {
            await _context.InscripcionesActividad.AddAsync(inscripcion);
            _context.Clases.Update(clase);
            await _context.SaveChangesAsync();
            return inscripcion;
        }

        public async Task<Asistencia?> ObtenerAsistenciaAsync(int claseId, int socioId, DateTime fecha)
        {
            var inicioDia = fecha.Date;
            var finDia = inicioDia.AddDays(1);

            return await _context.Asistencias
                .FirstOrDefaultAsync(a => a.ActividadId == claseId && a.SocioId == socioId && a.Fecha >= inicioDia && a.Fecha < finDia);
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

        public async Task<Profesor?> ObtenerProfesorPorIdAsync(int id)
        {
            return await _context.Profesores
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Clase> RegistrarClaseParticularAsync(Clase clase, Inscripcion_actividad inscripcion)
        {
            await _context.Clases.AddAsync(clase);
            await _context.InscripcionesActividad.AddAsync(inscripcion);
            await _context.SaveChangesAsync();
            return clase;
        }
    }
}