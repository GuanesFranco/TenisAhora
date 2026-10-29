using System;

namespace TenisAhora.Application.UseCases.Clases.Commands
{
    public class RegistrarAsistenciaClaseCommand
    {
        public int ClaseId { get; set; }
        public int DniUsuario { get; set; }
        public bool Presente { get; set; } = true;
        public DateTime? Fecha { get; set; }
    }
}