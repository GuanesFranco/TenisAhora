using System;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.DTOs
{
    public class TipoDeCanchaResponseDto
    {
        public int Id { get; set; }
        public Superficie Superficie { get; set; }
        public string SuperficieNombre => Superficie.ToString();
        public int Capacidad { get; set; }
        public double Precio { get; set; }
        public string? Caracteristicas { get; set; }
    }
}

