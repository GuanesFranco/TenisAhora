using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.DTOs
{
    public class EntrenadorResponseDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public int Dni { get; set; }
        public DateTime FechaDeNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int CodigoPostal { get; set; }
        public string Email { get; set; } = string.Empty;

        public string Especialidad { get; set; } = string.Empty;
        public int Antiguedad { get; set; }
        public int AdministradorId { get; set; }

        public ObjetivoEntrenamiento Objetivo { get; set; }
        public bool Activo { get; set; }

        public int? CertificacionId { get; set; }
        public string? NombreCertificacion { get; set; }
        public string? EnteEmisor { get; set; }
        public DateTime? FechaEmision { get; set; }
        public bool? CertificacionVerificada { get; set; }


    }
}
