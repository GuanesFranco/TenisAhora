using System;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.DTOs
{
    public class EntrenamientoParticularResponseDto
    {
        public int EntrenamientoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int EntrenadorId { get; set; }
        public string NombreEntrenador { get; set; } = string.Empty;
        public int SocioId { get; set; }
        public string NombreSocio { get; set; } = string.Empty;
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }
        public int Duracion { get; set; }
        public int InscripcionId { get; set; }
    }
}