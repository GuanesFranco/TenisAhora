using System;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.DTOs
{
    public class ClaseResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public TipoClase TipoDeClase { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }
        public int CapacidadMaxma { get; set; }
        public int CantidadDeAlumnos { get; set; }
        public int Duracion { get; set; }
        public int? ProfesorId { get; set; }
        public string? NombreProfesor { get; set; }
        public bool Activo { get; set; }
        public NivelActividad NivelActividad { get; set; }
    }
}