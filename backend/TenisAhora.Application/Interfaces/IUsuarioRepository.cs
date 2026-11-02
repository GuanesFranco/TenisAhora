using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Persona?> ObtenerPorIdAsync(int id);
    Task<Persona?> ObtenerPorDniAsync(int dni);
    Task<Persona?> ObtenerPorEmailAsync(string email);
    Task<bool> ExisteDniAsync(int dni, int? excluirId = null);
    Task<bool> ExisteEmailAsync(string email, int? excluirId = null);
    Task<List<Persona>> ListarTodosAsync(bool soloActivos = true);
    Task AgregarAsync(Persona persona, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Persona persona, CancellationToken cancellationToken = default);
    Task<Profesor?> ObtenerProfesorPorIdAsync(int id);
    Task<Entrenador?> ObtenerEntrenadorPorIdAsync(int id);
    Task<int?> ObtenerPrimerAdminIdAsync();
    Task RegistrarActividadParticularAsync(Actividad actividad, Inscripcion_actividad inscripcion, CancellationToken cancellationToken = default);
}
