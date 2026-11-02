using System;

namespace TenisAhora.Application.DTOs
{
    public class InscripcionClaseResponseDto
    {
        public int InscripcionId { get; set; }
        public int ClaseId { get; set; }
        public string NombreClase { get; set; } = string.Empty;
        public int SocioId { get; set; }
        public string NombreSocio { get; set; } = string.Empty;
        public string ApellidoSocio { get; set; } = string.Empty;
        public int DniSocio { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int CantidadDeAlumnosActual { get; set; }
        public int CapacidadMaxima { get; set; }
    }
}