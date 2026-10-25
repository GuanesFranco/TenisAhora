using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces
{
    public interface IClaseRepository
    {
        Task<Clase> CrearAsync(Clase clase);
        Task<Clase?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Clase>> ObtenerTodasAsync();
        Task ModificarAsync(Clase clase);
        Task EliminarAsync(int id);
        Task<Socio?> ObtenerSocioPorDniAsync(int dni);
        Task<bool> EstaInscriptoAsync(int claseId, int socioId);
        Task<Inscripcion_actividad> InscribirAlumnoAsync(Inscripcion_actividad inscripcion, Clase clase);
        Task<Asistencia?> ObtenerAsistenciaAsync(int claseId, int socioId, DateTime fecha);
        Task<Asistencia> RegistrarAsistenciaAsync(Asistencia asistencia);
    }
}