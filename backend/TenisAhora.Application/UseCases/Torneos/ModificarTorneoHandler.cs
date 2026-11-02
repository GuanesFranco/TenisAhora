using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Torneos;

public class ModificarTorneoHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public ModificarTorneoHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task<TorneoResponseDto> Handle(int id, ModificarTorneoCommand command, CancellationToken cancellationToken = default)
    {
        var torneo = await _competenciaRepository.ObtenerTorneoPorIdAsync(id);
        if (torneo is null)
        {
            throw new DomainException($"No se encontró ningún torneo con ID {id} para modificar.");
        }

        if (command.FechaFin < command.FechaInicio)
        {
            throw new DomainException("La fecha de finalización no puede ser anterior a la de inicio.");
        }

        torneo.Nombre = command.Nombre.Trim();
        torneo.FechaInicio = command.FechaInicio;
        torneo.FechaFin = command.FechaFin;
        torneo.categoriaGenero = command.CategoriaGenero;
        torneo.modalidad = command.Modalidad;
        if (!string.IsNullOrWhiteSpace(command.TipoLlave))
        {
            torneo.tipo_llave = command.TipoLlave;
        }

        await _competenciaRepository.ActualizarTorneoAsync(torneo, cancellationToken);
        return ObtenerTorneoHandler.MapearTorneo(torneo);
    }
}

public class EliminarTorneoHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public EliminarTorneoHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task Handle(int id, CancellationToken cancellationToken = default)
    {
        var torneo = await _competenciaRepository.ObtenerTorneoPorIdAsync(id);
        if (torneo is null)
        {
            throw new DomainException($"No se encontró ningún torneo con ID {id} para eliminar.");
        }

        // Validación RF-31: No eliminar si tiene partidos disputados o en desarrollo
        var tienePartidosJugados = torneo.Partidos.Any(p => !string.IsNullOrWhiteSpace(p.resultado) && p.resultado != "Pendiente");
        if (tienePartidosJugados)
        {
            throw new DomainException("No se puede eliminar el torneo porque ya posee partidos en desarrollo o finalizados.");
        }

        torneo.Activo = false;
        await _competenciaRepository.ActualizarTorneoAsync(torneo, cancellationToken);
    }
}
