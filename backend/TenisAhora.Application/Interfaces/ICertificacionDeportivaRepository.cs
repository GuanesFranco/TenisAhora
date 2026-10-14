using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.Interfaces
{
    public interface ICertificacionDeportivaRepository
    {
        Task<int> CreateAsync(CertificacionDeportiva certificacion);
        Task<bool> InstructorExistsAsync(int instructorId);

        Task<bool> ResolverAsync( int certificacionId, EstadoCertificacion estado);
    }
}
