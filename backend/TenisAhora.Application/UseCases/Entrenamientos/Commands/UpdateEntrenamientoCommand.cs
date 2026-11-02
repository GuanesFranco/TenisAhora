using System;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Entrenamientos.Commands
{
    public class UpdateEntrenamientoCommand
    {
        public string Nombre { get; set; } = string.Empty;
        public ObjetivoEntrenamiento ObjetivoEntrenamiento { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }
        public int CapacidadMaxma { get; set; } // Cupo máximo (tope 10)
        public int Duracion { get; set; }
        public int? EntrenadorId { get; set; }
        public NivelActividad NivelActividad { get; set; }
    }
}