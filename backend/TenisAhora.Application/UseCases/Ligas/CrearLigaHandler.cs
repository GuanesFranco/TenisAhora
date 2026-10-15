using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Ligas;

public class CrearLigaHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public CrearLigaHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task<LigaResponseDto> Handle(CrearLigaCommand command, CancellationToken cancellationToken = default)
    {
        if (command.FechaFin < command.FechaInicio)
        {
            throw new DomainException("La fecha de finalización no puede ser anterior a la de inicio.");
        }

        var liga = new Liga
        {
            Nombre = command.Nombre.Trim(),
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin,
            categoriaGenero = command.CategoriaGenero,
            modalidad = command.Modalidad,
            estado = EstadoCompetencia.EnCurso,
            Cantidad_fechas = command.CantidadFechas,
            puntos_por_partido = command.PuntosPorPartido,
            ReglamentoId = command.ReglamentoId > 0 ? command.ReglamentoId : 1,
            Activo = true
        };

        await _competenciaRepository.AgregarLigaAsync(liga, cancellationToken);

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
            0,
            new List<InscriptoDto>(),
            new List<PartidoDto>()
        );
    }
}
