using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces;

public interface ICompetenciaRepository
{
    // Torneos
    Task<Torneo?> ObtenerTorneoPorIdAsync(int id);
    Task<List<Torneo>> ListarTorneosAsync(bool soloActivos = true);
    Task AgregarTorneoAsync(Torneo torneo, CancellationToken cancellationToken = default);
    Task ActualizarTorneoAsync(Torneo torneo, CancellationToken cancellationToken = default);

    // Ligas
    Task<Liga?> ObtenerLigaPorIdAsync(int id);
    Task<List<Liga>> ListarLigasAsync(bool soloActivos = true);
    Task AgregarLigaAsync(Liga liga, CancellationToken cancellationToken = default);
    Task ActualizarLigaAsync(Liga liga, CancellationToken cancellationToken = default);

    // Inscripciones a Torneos y Ligas
    Task<bool> EstaInscriptoAsync(int competenciaId, int socioId);
    Task InscribirCompetenciaAsync(Inscripcion_competencia inscripcion, CancellationToken cancellationToken = default);
    Task<List<Inscripcion_competencia>> ObtenerInscripcionesAsync(int competenciaId);

    // Partidos y Fixtures
    Task<Partido?> ObtenerPartidoPorIdAsync(int partidoId);
    Task GuardarPartidosAsync(List<Partido> partidos, CancellationToken cancellationToken = default);
    Task ActualizarPartidoAsync(Partido partido, CancellationToken cancellationToken = default);
    Task<List<Partido>> ObtenerPartidosPorCompetenciaAsync(int competenciaId);
}
