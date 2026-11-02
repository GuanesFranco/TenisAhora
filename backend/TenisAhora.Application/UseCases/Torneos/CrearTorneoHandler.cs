using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Torneos;

public class CrearTorneoHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public CrearTorneoHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task<TorneoResponseDto> Handle(CrearTorneoCommand command, CancellationToken cancellationToken = default)
    {
        if (command.FechaFin < command.FechaInicio)
        {
            throw new DomainException("La fecha de finalización no puede ser anterior a la fecha de inicio.");
        }

        var torneo = new Torneo
        {
            Nombre = command.Nombre.Trim(),
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin,
            categoriaGenero = command.CategoriaGenero,
            modalidad = command.Modalidad,
            estado = EstadoCompetencia.EnCurso,
            etapa_actual = "Inscripción Abierta",
            tipo_llave = string.IsNullOrWhiteSpace(command.TipoLlave) ? "Eliminación Simple" : command.TipoLlave,
            ReglamentoId = command.ReglamentoId > 0 ? command.ReglamentoId : 1,
            Activo = true
        };

        await _competenciaRepository.AgregarTorneoAsync(torneo, cancellationToken);

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
            0,
            new List<InscriptoDto>(),
            new List<PartidoDto>()
        );
    }
}
