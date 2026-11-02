using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.DTOs.Usuarios;

public record UsuarioResponseDto(
    int Id,
    string Nombre,
    string Apellido,
    int Dni,
    string Email,
    string Telefono,
    string Direccion,
    int CodigoPostal,
    DateTime FechaDeNacimiento,
    Rol Rol,
    bool Activo,
    EstadoMembresia? EstadoMembresia = null,
    int? CertificacionId = null,
    string? TituloHabilitante = null,
    string? EnteEmisor = null,
    EstadoCertificacion? EstadoCertificacion = null
);

public record FichaUsuarioDto(
    int Id,
    string NombreCompleto,
    int Dni,
    string Email,
    string Telefono,
    string Direccion,
    int CodigoPostal,
    DateTime FechaDeNacimiento,
    int Edad,
    string Rol,
    string? EstadoMembresia,
    DateTime FechaEmisionReporte
);

public record AsignacionResponseDto(
    int ActividadId,
    int SocioId,
    string SocioNombre,
    int InstructorId,
    string InstructorNombre,
    string TipoInstructor,
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin,
    string Mensaje
);
