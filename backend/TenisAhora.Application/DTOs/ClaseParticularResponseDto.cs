using System;

namespace TenisAhora.Application.DTOs
{
    public class ClaseParticularResponseDto
    {
        public int ClaseId { get; set; }
        public string NombreClase { get; set; } = string.Empty;
        public int ProfesorId { get; set; }
        public string NombreProfesor { get; set; } = string.Empty;
        public int SocioId { get; set; }
        public string NombreSocio { get; set; } = string.Empty;
        public int DniSocio { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }
        public int Duracion { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}