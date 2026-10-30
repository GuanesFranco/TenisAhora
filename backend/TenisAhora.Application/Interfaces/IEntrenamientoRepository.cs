using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces
{
    public interface IEntrenamientoRepository
    {
        Task<Entrenamiento> CrearAsync(Entrenamiento entrenamiento);
        Task<Entrenamiento?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Entrenamiento>> ObtenerTodosAsync();
        Task ModificarAsync(Entrenamiento entrenamiento);
        Task EliminarAsync(int id);
        Task<Socio?> ObtenerSocioPorDniAsync(int dni);
        Task<bool> EstaInscriptoAsync(int entrenamientoId, int socioId);
        Task<Inscripcion_actividad> InscribirAlumnoAsync(Inscripcion_actividad inscripcion, Entrenamiento entrenamiento);
        Task<Asistencia?> ObtenerAsistenciaAsync(int entrenamientoId, int socioId, DateTime fecha);
        Task<Asistencia> RegistrarAsistenciaAsync(Asistencia asistencia);
        Task<Entrenador?> ObtenerEntrenadorPorIdAsync(int id);
        Task<Entrenamiento> RegistrarEntrenamientoParticularAsync(Entrenamiento entrenamiento, Inscripcion_actividad inscripcion);
    }
}