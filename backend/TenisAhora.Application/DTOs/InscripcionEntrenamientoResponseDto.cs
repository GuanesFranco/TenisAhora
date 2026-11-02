using System;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.DTOs
{
    public class InscripcionEntrenamientoResponseDto
    {
        public int Id { get; set; }
        public int EntrenamientoId { get; set; }
        public int SocioId { get; set; }
        public string NombreSocio { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public EstadoInscripcion Estado { get; set; }
        public int CantidadAlumnosActual { get; set; }
        public int CapacidadMaxima { get; set; }
    }
}