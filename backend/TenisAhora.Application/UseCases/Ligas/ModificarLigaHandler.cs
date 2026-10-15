using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Ligas;

public class ModificarLigaHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public ModificarLigaHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task<LigaResponseDto> Handle(int id, ModificarLigaCommand command, CancellationToken cancellationToken = default)
    {
        var liga = await _competenciaRepository.ObtenerLigaPorIdAsync(id);
        if (liga is null)
        {
            throw new DomainException($"No se encontró ninguna liga con ID {id} para modificar.");
        }

        if (command.FechaFin < command.FechaInicio)
        {
            throw new DomainException("La fecha de finalización no puede ser anterior a la de inicio.");
        }

        liga.Nombre = command.Nombre.Trim();
        liga.FechaInicio = command.FechaInicio;
        liga.FechaFin = command.FechaFin;
        liga.categoriaGenero = command.CategoriaGenero;
        liga.modalidad = command.Modalidad;
        liga.Cantidad_fechas = command.CantidadFechas;
        liga.puntos_por_partido = command.PuntosPorPartido;

        await _competenciaRepository.ActualizarLigaAsync(liga, cancellationToken);
        return ObtenerLigaHandler.MapearLiga(liga);
    }
}

public class EliminarLigaHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public EliminarLigaHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task Handle(int id, CancellationToken cancellationToken = default)
    {
        var liga = await _competenciaRepository.ObtenerLigaPorIdAsync(id);
        if (liga is null)
        {
            throw new DomainException($"No se encontró ninguna liga con ID {id} para eliminar.");
        }

        // Validación RF-38: No eliminar si ya tiene partidos jugados o en disputa
        var tienePartidosEnDisputa = liga.Partidos.Any(p => !string.IsNullOrWhiteSpace(p.resultado) && p.resultado != "Pendiente");
        if (tienePartidosEnDisputa)
        {
            throw new DomainException("No se puede eliminar la liga porque se encuentra en fase activa con partidos disputados.");
        }

        liga.Activo = false;
        await _competenciaRepository.ActualizarLigaAsync(liga, cancellationToken);
    }
}
