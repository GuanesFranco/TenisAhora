using System;

namespace TenisAhora.Application.DTOs
{
    public class AsistenciaClaseResponseDto
    {
        public int AsistenciaId { get; set; }
        public int ClaseId { get; set; }
        public string NombreClase { get; set; } = string.Empty;
        public int SocioId { get; set; }
        public string NombreSocio { get; set; } = string.Empty;
        public string ApellidoSocio { get; set; } = string.Empty;
        public int DniSocio { get; set; }
        public DateTime Fecha { get; set; }
        public bool Presente { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }
}