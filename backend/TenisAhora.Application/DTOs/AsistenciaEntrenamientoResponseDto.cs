using System;

namespace TenisAhora.Application.DTOs
{
    public class AsistenciaEntrenamientoResponseDto
    {
        public int Id { get; set; }
        public int EntrenamientoId { get; set; }
        public int SocioId { get; set; }
        public string NombreSocio { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public bool Presente { get; set; }
    }
}