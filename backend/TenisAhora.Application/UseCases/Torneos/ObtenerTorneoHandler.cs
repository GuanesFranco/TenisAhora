using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Torneos;

public class ObtenerTorneoHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public ObtenerTorneoHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task<TorneoResponseDto> Handle(int id)
    {
        var torneo = await _competenciaRepository.ObtenerTorneoPorIdAsync(id);
        if (torneo is null)
        {
            throw new DomainException($"No se encontró ningún torneo con ID {id}.");
        }

        return MapearTorneo(torneo);
    }

    public static TorneoResponseDto MapearTorneo(Torneo torneo)
    {
        var inscriptos = torneo.Inscripciones.Select(i => new InscriptoDto(
            i.Id,
            i.SocioId,
            i.Socio != null ? $"{i.Socio.Nombre} {i.Socio.Apellido}" : $"Socio #{i.SocioId}",
            i.Socio != null ? i.Socio.Dni : 0,
            i.NumeroParticipante,
            i.Fecha
        )).ToList();

        var partidos = torneo.Partidos.Select(p => new PartidoDto(
            p.Id,
            p.fecha,
            p.HoraInicio,
            p.HoraFin,
            p.ronda,
            p.resultado,
            p.CanchaId
        )).ToList();

        return new TorneoResponseDto(
            torneo.Id,
            torneo.Nombre,
            torneo.FechaInicio,
            torneo.FechaFin,
            torneo.categoriaGenero.ToString(),
            torneo.modalidad.ToString(),
            torneo.estado.ToString(),
            torneo.etapa_actual,
            torneo.tipo_llave,
            torneo.Activo,
            inscriptos.Count,
            inscriptos,
            partidos
        );
    }
}

public class ListarTorneosHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public ListarTorneosHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task<List<TorneoResponseDto>> Handle(bool soloActivos = true)
    {
        var torneos = await _competenciaRepository.ListarTorneosAsync(soloActivos);
        return torneos.Select(ObtenerTorneoHandler.MapearTorneo).ToList();
    }
}
