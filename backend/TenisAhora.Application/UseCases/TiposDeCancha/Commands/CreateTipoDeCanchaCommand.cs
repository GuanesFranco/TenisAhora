using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.TiposDeCancha.Commands
{
    public class CreateTipoDeCanchaCommand
    {
        public Superficie Superficie { get; set; }
        public int Capacidad { get; set; }
        public double Precio { get; set; }
        public string? Caracteristicas { get; set; }
    }
}

