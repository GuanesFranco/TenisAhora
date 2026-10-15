using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Torneos;

public class GenerarFixtureTorneoHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public GenerarFixtureTorneoHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task<List<PartidoDto>> Handle(int torneoId, CancellationToken cancellationToken = default)
    {
        var torneo = await _competenciaRepository.ObtenerTorneoPorIdAsync(torneoId);
        if (torneo is null || !torneo.Activo)
        {
            throw new DomainException($"El torneo con ID {torneoId} no existe o no se encuentra activo.");
        }

        if (torneo.Inscripciones.Count < 2)
        {
            throw new DomainException("Se requieren al menos 2 participantes inscriptos para generar los cruces del torneo.");
        }

        if (torneo.Partidos.Any())
        {
            throw new DomainException("El torneo ya posee un cuadro de partidos generado previamente.");
        }

        var inscriptos = torneo.Inscripciones.OrderBy(i => i.NumeroParticipante).ToList();
        var partidos = new List<Partido>();
        var fechaPartido = torneo.FechaInicio;

        string nombreRonda;
        if (inscriptos.Count == 2) nombreRonda = "Final";
        else if (inscriptos.Count <= 4) nombreRonda = "Semifinales";
        else if (inscriptos.Count <= 8) nombreRonda = "Cuartos de Final";
        else nombreRonda = "Primera Ronda";

        for (int i = 0; i < inscriptos.Count; i += 2)
        {
            if (i + 1 < inscriptos.Count)
            {
                var j1 = inscriptos[i].Socio != null ? $"{inscriptos[i].Socio.Nombre} {inscriptos[i].Socio.Apellido}" : $"Jugador {i + 1}";
                var j2 = inscriptos[i + 1].Socio != null ? $"{inscriptos[i + 1].Socio.Nombre} {inscriptos[i + 1].Socio.Apellido}" : $"Jugador {i + 2}";

                partidos.Add(new Partido
                {
                    CompetenciaId = torneo.Id,
                    fecha = fechaPartido,
                    HoraInicio = fechaPartido.Date.AddHours(10 + (i * 2)),
                    HoraFin = fechaPartido.Date.AddHours(12 + (i * 2)),
                    ronda = $"{nombreRonda} ({j1} vs {j2})",
                    resultado = "Pendiente",
                    CanchaId = 1
                });
            }
        }

        torneo.etapa_actual = $"En disputa ({nombreRonda})";
        await _competenciaRepository.GuardarPartidosAsync(partidos, cancellationToken);
        await _competenciaRepository.ActualizarTorneoAsync(torneo, cancellationToken);

        return partidos.Select(p => new PartidoDto(
            p.Id,
            p.fecha,
            p.HoraInicio,
            p.HoraFin,
            p.ronda,
            p.resultado,
            p.CanchaId
        )).ToList();
    }
}

public class RegistrarResultadoTorneoHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public RegistrarResultadoTorneoHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task<PartidoDto> Handle(int partidoId, RegistrarResultadoPartidoCommand command, CancellationToken cancellationToken = default)
    {
        var partido = await _competenciaRepository.ObtenerPartidoPorIdAsync(partidoId);
        if (partido is null)
        {
            throw new DomainException($"No se encontró ningún partido con ID {partidoId}.");
        }

        partido.resultado = string.IsNullOrWhiteSpace(command.GanadorNombre)
            ? command.Resultado.Trim()
            : $"{command.Resultado.Trim()} (Ganador: {command.GanadorNombre.Trim()})";

        await _competenciaRepository.ActualizarPartidoAsync(partido, cancellationToken);

        return new PartidoDto(
            partido.Id,
            partido.fecha,
            partido.HoraInicio,
            partido.HoraFin,
            partido.ronda,
            partido.resultado,
            partido.CanchaId
        );
    }
}
