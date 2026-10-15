using NSubstitute;
using TenisAhora.Application.DTOs.Competencias;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Ligas;
using TenisAhora.Application.UseCases.Torneos;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.UnitTests;

public class CompetenciasTests
{
    private readonly ICompetenciaRepository _competenciaRepo = Substitute.For<ICompetenciaRepository>();
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();

    [Fact]
    public async Task CrearTorneo_ConFechaFinAnteriorAInicio_LanzaDomainException()
    {
        // Arrange
        var handler = new CrearTorneoHandler(_competenciaRepo);
        var command = new CrearTorneoCommand(
            Nombre: "Copa Primavera",
            FechaInicio: new DateTime(2026, 11, 10),
            FechaFin: new DateTime(2026, 11, 5), // fecha inválida
            CategoriaGenero: CategoriaGenero.Masculino,
            Modalidad: Modalidad.Single
        );

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command));
        await _competenciaRepo.DidNotReceive().AgregarTorneoAsync(Arg.Any<Torneo>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerarFixtureTorneo_ConMenosDeDosInscriptos_LanzaDomainException()
    {
        // Arrange: torneo con solo 1 inscripto
        var torneoId = 1;
        var torneo = new Torneo
        {
            Id = torneoId,
            Nombre = "Torneo Single",
            Activo = true,
            Inscripciones = new List<Inscripcion_competencia>
            {
                new Inscripcion_competencia { Id = 1, NumeroParticipante = 1 }
            }
        };

        _competenciaRepo.ObtenerTorneoPorIdAsync(torneoId).Returns(torneo);
        var handler = new GenerarFixtureTorneoHandler(_competenciaRepo);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(torneoId));
        Assert.Contains("al menos 2 participantes", ex.Message);
    }

    [Fact]
    public async Task GenerarFixtureLiga_GeneraPartidosTodosContraTodosCorrectamente()
    {
        // Arrange: liga con 3 participantes -> debe generar 3 partidos (1v2, 1v3, 2v3)
        var ligaId = 5;
        var liga = new Liga
        {
            Id = ligaId,
            Nombre = "Liga Anual",
            FechaInicio = new DateTime(2026, 10, 1),
            FechaFin = new DateTime(2026, 12, 1),
            Activo = true,
            puntos_por_partido = 3,
            Inscripciones = new List<Inscripcion_competencia>
            {
                new Inscripcion_competencia { Id = 1, NumeroParticipante = 1, Socio = new Socio { Nombre = "Juan", Apellido = "Perez" } },
                new Inscripcion_competencia { Id = 2, NumeroParticipante = 2, Socio = new Socio { Nombre = "Pedro", Apellido = "Gomez" } },
                new Inscripcion_competencia { Id = 3, NumeroParticipante = 3, Socio = new Socio { Nombre = "Lucas", Apellido = "Diaz" } }
            }
        };

        _competenciaRepo.ObtenerLigaPorIdAsync(ligaId).Returns(liga);
        var handler = new GenerarFixtureLigaHandler(_competenciaRepo);

        // Act
        var partidos = await handler.Handle(ligaId);

        // Assert
        Assert.Equal(3, partidos.Count);
        await _competenciaRepo.Received(1).GuardarPartidosAsync(
            Arg.Is<List<Partido>>(list => list.Count == 3),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ObtenerTablaPosiciones_CalculaPuntosYOrdenaCorrectamente()
    {
        // Arrange: Liga con 2 jugadores y un partido ganado por Juan
        var ligaId = 7;
        var jugador1 = new Socio { Id = 10, Nombre = "Juan", Apellido = "Perez" };
        var jugador2 = new Socio { Id = 20, Nombre = "Carlos", Apellido = "Alcaraz" };

        var liga = new Liga
        {
            Id = ligaId,
            puntos_por_partido = 3,
            Inscripciones = new List<Inscripcion_competencia>
            {
                new Inscripcion_competencia { SocioId = 10, Socio = jugador1, NumeroParticipante = 1 },
                new Inscripcion_competencia { SocioId = 20, Socio = jugador2, NumeroParticipante = 2 }
            },
            Partidos = new List<Partido>
            {
                new Partido
                {
                    Id = 1,
                    ronda = "Jornada 1 (Juan Perez vs Carlos Alcaraz)",
                    resultado = "6-4 6-3 (Ganador: Juan Perez)"
                }
            }
        };

        _competenciaRepo.ObtenerLigaPorIdAsync(ligaId).Returns(liga);
        var handler = new ObtenerTablaPosicionesHandler(_competenciaRepo);

        // Act
        var tabla = await handler.Handle(ligaId);

        // Assert
        Assert.Equal(2, tabla.Count);
        // Juan debe quedar primero con 3 puntos
        Assert.Equal(1, tabla[0].Posicion);
        Assert.Equal(10, tabla[0].SocioId);
        Assert.Equal(3, tabla[0].Puntos);
        Assert.Equal(1, tabla[0].PartidosGanados);

        // Carlos debe quedar segundo con 0 puntos
        Assert.Equal(2, tabla[1].Posicion);
        Assert.Equal(20, tabla[1].SocioId);
        Assert.Equal(0, tabla[1].Puntos);
        Assert.Equal(1, tabla[1].PartidosPerdidos);
    }
}
