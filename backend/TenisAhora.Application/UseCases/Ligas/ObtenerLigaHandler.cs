using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Ligas;

public class ObtenerLigaHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public ObtenerLigaHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task<LigaResponseDto> Handle(int id)
    {
        var liga = await _competenciaRepository.ObtenerLigaPorIdAsync(id);
        if (liga is null)
        {
            throw new DomainException($"No se encontró ninguna liga con ID {id}.");
        }

        return MapearLiga(liga);
    }

    public static LigaResponseDto MapearLiga(Liga liga)
    {
        var inscriptos = liga.Inscripciones.Select(i => new InscriptoDto(
            i.Id,
            i.SocioId,
            i.Socio != null ? $"{i.Socio.Nombre} {i.Socio.Apellido}" : $"Socio #{i.SocioId}",
            i.Socio != null ? i.Socio.Dni : 0,
            i.NumeroParticipante,
            i.Fecha
        )).ToList();

        var partidos = liga.Partidos.Select(p => new PartidoDto(
            p.Id,
            p.fecha,
            p.HoraInicio,
            p.HoraFin,
            p.ronda,
            p.resultado,
            p.CanchaId
        )).ToList();

        return new LigaResponseDto(
            liga.Id,
            liga.Nombre,
            liga.FechaInicio,
            liga.FechaFin,
            liga.categoriaGenero.ToString(),
            liga.modalidad.ToString(),
            liga.estado.ToString(),
            liga.Cantidad_fechas,
            liga.puntos_por_partido,
            liga.Activo,
            inscriptos.Count,
            inscriptos,
            partidos
        );
    }
}

public class ListarLigasHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public ListarLigasHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task<List<LigaResponseDto>> Handle(bool soloActivos = true)
    {
        var ligas = await _competenciaRepository.ListarLigasAsync(soloActivos);
        return ligas.Select(ObtenerLigaHandler.MapearLiga).ToList();
    }
}
