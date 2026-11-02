using System;

namespace TenisAhora.Application.UseCases.Entrenamientos.Commands
{
    public class RegistrarAsistenciaEntrenamientoCommand
    {
        public int EntrenamientoId { get; set; }
        public int Dni { get; set; }
        public bool Presente { get; set; } = true;
        public DateTime? Fecha { get; set; }
    }
}