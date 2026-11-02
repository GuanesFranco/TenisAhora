using System;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Clases.Commands
{
    public class AsignarProfesorParticularCommand
    {
        public int DniUsuario { get; set; }
        public int ProfesorId { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public int DuracionMinutos { get; set; } = 60;
        public TipoClase TipoDeClase { get; set; } = TipoClase.Tecnica;
        public NivelActividad Nivel { get; set; } = NivelActividad.Principiante;
    }
}