using System;

namespace TenisAhora.Application.UseCases.Entrenamientos.Commands
{
    public class AsignarEntrenadorParticularCommand
    {
        public int Dni { get; set; }
        public int EntrenadorId { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }
        public int Duracion { get; set; }
        public string? Nombre { get; set; }
    }
}