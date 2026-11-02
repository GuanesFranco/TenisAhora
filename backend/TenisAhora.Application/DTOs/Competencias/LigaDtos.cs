using System.ComponentModel.DataAnnotations;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.DTOs.Competencias;

public record CrearLigaCommand(
    [Required(ErrorMessage = "El nombre de la liga es obligatorio")]
    string Nombre,
    [Required] DateTime FechaInicio,
    [Required] DateTime FechaFin,
    CategoriaGenero CategoriaGenero,
    Modalidad Modalidad,
    [Range(1, 38, ErrorMessage = "La cantidad de fechas debe ser entre 1 y 38")] int CantidadFechas = 10,
    [Range(1, 10, ErrorMessage = "Los puntos por victoria deben ser al menos 1")] int PuntosPorPartido = 3,
    int ReglamentoId = 1
);

public record ModificarLigaCommand(
    [Required(ErrorMessage = "El nombre de la liga es obligatorio")]
    string Nombre,
    [Required] DateTime FechaInicio,
    [Required] DateTime FechaFin,
    CategoriaGenero CategoriaGenero,
    Modalidad Modalidad,
    int CantidadFechas,
    int PuntosPorPartido
);

public record InscribirLigaCommand(
    [Required] int SocioDni
);

public record LigaResponseDto(
    int Id,
    string Nombre,
    DateTime FechaInicio,
    DateTime FechaFin,
    string CategoriaGenero,
    string Modalidad,
    string Estado,
    int CantidadFechas,
    int PuntosPorPartido,
    bool Activo,
    int CantidadInscriptos,
    List<InscriptoDto> Inscriptos,
    List<PartidoDto> Partidos
);

public record TablaPosicionesItemDto(
    int Posicion,
    int SocioId,
    string NombreJugador,
    int PartidosJugados,
    int PartidosGanados,
    int PartidosPerdidos,
    int Puntos
);
