using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.Ligas;

public class GenerarFixtureLigaHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public GenerarFixtureLigaHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task<List<PartidoDto>> Handle(int ligaId, CancellationToken cancellationToken = default)
    {
        var liga = await _competenciaRepository.ObtenerLigaPorIdAsync(ligaId);
        if (liga is null || !liga.Activo)
        {
            throw new DomainException($"La liga con ID {ligaId} no existe o no se encuentra activa.");
        }

        if (liga.Inscripciones.Count < 2)
        {
            throw new DomainException("Se requieren al menos 2 participantes inscriptos para generar el fixture de la liga.");
        }

        if (liga.Partidos.Any())
        {
            throw new DomainException("La liga ya posee un fixture de partidos generado previamente.");
        }

        var inscriptos = liga.Inscripciones.OrderBy(i => i.NumeroParticipante).ToList();
        var partidos = new List<Partido>();
        int jornada = 1;

        // Algoritmo Round-Robin (Todos contra todos)
        for (int i = 0; i < inscriptos.Count; i++)
        {
            for (int j = i + 1; j < inscriptos.Count; j++)
            {
                var j1 = inscriptos[i].Socio != null ? $"{inscriptos[i].Socio.Nombre} {inscriptos[i].Socio.Apellido}" : $"Jugador {i + 1}";
                var j2 = inscriptos[j].Socio != null ? $"{inscriptos[j].Socio.Nombre} {inscriptos[j].Socio.Apellido}" : $"Jugador {j + 1}";

                var fechaProgramada = liga.FechaInicio.AddDays((jornada - 1) * 7);

                partidos.Add(new Partido
                {
                    CompetenciaId = liga.Id,
                    fecha = fechaProgramada,
                    HoraInicio = fechaProgramada.Date.AddHours(14),
                    HoraFin = fechaProgramada.Date.AddHours(16),
                    ronda = $"Jornada {jornada} ({j1} vs {j2})",
                    resultado = "Pendiente",
                    CanchaId = 1
                });

                jornada++;
            }
        }

        await _competenciaRepository.GuardarPartidosAsync(partidos, cancellationToken);

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

public class RegistrarResultadoLigaHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public RegistrarResultadoLigaHandler(ICompetenciaRepository competenciaRepository)
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

public class ObtenerTablaPosicionesHandler
{
    private readonly ICompetenciaRepository _competenciaRepository;

    public ObtenerTablaPosicionesHandler(ICompetenciaRepository competenciaRepository)
    {
        _competenciaRepository = competenciaRepository;
    }

    public async Task<List<TablaPosicionesItemDto>> Handle(int ligaId)
    {
        var liga = await _competenciaRepository.ObtenerLigaPorIdAsync(ligaId);
        if (liga is null)
        {
            throw new DomainException($"No se encontró la liga con ID {ligaId}.");
        }

        var inscriptos = liga.Inscripciones;
        var partidos = liga.Partidos;

        var tabla = new List<TablaPosicionesItemDto>();

        foreach (var inscripto in inscriptos)
        {
            var nombreCompleto = inscripto.Socio != null
                ? $"{inscripto.Socio.Nombre} {inscripto.Socio.Apellido}"
                : $"Jugador #{inscripto.NumeroParticipante}";

            // Partidos en los que participó
            var partidosDelJugador = partidos.Where(p => p.ronda != null && p.ronda.Contains(nombreCompleto)).ToList();
            var jugados = partidosDelJugador.Count(p => !string.IsNullOrWhiteSpace(p.resultado) && p.resultado != "Pendiente");

            // Partidos donde figura como ganador
            var ganados = partidosDelJugador.Count(p => p.resultado != null && p.resultado.Contains($"Ganador: {nombreCompleto}"));
            var perdidos = jugados - ganados;
            var puntos = ganados * liga.puntos_por_partido;

            tabla.Add(new TablaPosicionesItemDto(
                0, // se calcula luego de ordenar
                inscripto.SocioId,
                nombreCompleto,
                jugados,
                ganados,
                perdidos,
                puntos
            ));
        }

        // Ordenar por puntos descendente
        var tablaOrdenada = tabla.OrderByDescending(t => t.Puntos).ThenByDescending(t => t.PartidosGanados).ToList();

        // Asignar posición
        return tablaOrdenada.Select((item, index) => item with { Posicion = index + 1 }).ToList();
    }
}
