using System.ComponentModel.DataAnnotations;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.DTOs.Competencias;

public record CrearTorneoCommand(
    [Required(ErrorMessage = "El nombre del torneo es obligatorio")]
    string Nombre,
    [Required] DateTime FechaInicio,
    [Required] DateTime FechaFin,
    CategoriaGenero CategoriaGenero,
    Modalidad Modalidad,
    string TipoLlave = "Eliminatoria",
    int ReglamentoId = 1
);

public record ModificarTorneoCommand(
    [Required(ErrorMessage = "El nombre del torneo es obligatorio")]
    string Nombre,
    [Required] DateTime FechaInicio,
    [Required] DateTime FechaFin,
    CategoriaGenero CategoriaGenero,
    Modalidad Modalidad,
    string TipoLlave
);

public record InscribirTorneoCommand(
    [Required] int SocioDni
);

public record TorneoResponseDto(
    int Id,
    string Nombre,
    DateTime FechaInicio,
    DateTime FechaFin,
    string CategoriaGenero,
    string Modalidad,
    string Estado,
    string EtapaActual,
    string TipoLlave,
    bool Activo,
    int CantidadInscriptos,
    List<InscriptoDto> Inscriptos,
    List<PartidoDto> Partidos
);

public record InscriptoDto(
    int InscripcionId,
    int SocioId,
    string NombreCompleto,
    int Dni,
    int NumeroParticipante,
    DateTime FechaInscripcion
);

public record PartidoDto(
    int Id,
    DateTime Fecha,
    DateTime HoraInicio,
    DateTime HoraFin,
    string Ronda,
    string? Resultado,
    int CanchaId
);

public record RegistrarResultadoPartidoCommand(
    [Required(ErrorMessage = "El resultado es obligatorio (ej. 6-4, 6-3)")]
    string Resultado,
    string? GanadorNombre = null
);
