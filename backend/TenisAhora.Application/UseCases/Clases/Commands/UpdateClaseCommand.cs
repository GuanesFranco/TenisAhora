using System;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Clases.Commands
{
    public class UpdateClaseCommand
    {
        public string Nombre { get; set; } = string.Empty;
        public TipoClase TipoDeClase { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }
        public int CapacidadMaxma { get; set; } // Validación cupo máx 30
        public int Duracion { get; set; }
        public int? ProfesorId { get; set; }
        public NivelActividad NivelActividad { get; set; }
    }
}