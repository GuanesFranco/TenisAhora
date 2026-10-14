using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Infrastructure.Persistence;

namespace TenisAhora.Infrastructure.Repository
{
    public class CertificacionDeportivaRepository : ICertificacionDeportivaRepository
    {


        private readonly AppDbContext _context;

        public CertificacionDeportivaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> InstructorExistsAsync(int instructorId)
        {
            return await _context.Instructores
                .AnyAsync(i => i.Id == instructorId);
        }

        public async Task<int> CreateAsync(
            CertificacionDeportiva certificacion)
        {
            _context.CertificacionesDeportivas.Add(certificacion);

            await _context.SaveChangesAsync();

            return certificacion.Id;
        }




        public async Task<bool> ResolverAsync( int certificacionId, EstadoCertificacion estado)
        {
            var certificacion = await _context
                .Set<CertificacionDeportiva>()
                .Include(c => c.instructor)
                .FirstOrDefaultAsync(c => c.Id == certificacionId);

            if (certificacion is null)
                return false;

            if (certificacion.instructor is null)
                return false;

            if (estado == EstadoCertificacion.Aprobada)
            {
                certificacion.EstadoCertificacion = EstadoCertificacion.Aprobada;
                certificacion.certificacionDeportiva = true;
                certificacion.instructor.Activo = true;
            }
            else if (estado == EstadoCertificacion.Rechazada)
            {
                certificacion.EstadoCertificacion = EstadoCertificacion.Rechazada;
                certificacion.certificacionDeportiva = false;
                certificacion.instructor.Activo = false;
            }
            else
            {
                return false;
            }

            await _context.SaveChangesAsync();

            return true;
        }



    }


}

