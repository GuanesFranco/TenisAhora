using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Application.DTOs.Canchas
{
    public class CanchaResponseDto
    {
        public int Id { get; set; }

        public int TipoDeCanchaId { get; set; }

        public string Superficie { get; set; } = string.Empty;

        public int Capacidad { get; set; }

        public double Precio { get; set; }

        public string? FotoUrl { get; set; }
    }
}
