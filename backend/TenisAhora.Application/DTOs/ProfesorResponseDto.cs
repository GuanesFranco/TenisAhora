using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.DTOs
{
    public class ProfesorResponseDto
    {
        public int Id { get; set; }

        // Datos personales
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public int Dni { get; set; }
        public DateTime FechaDeNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int CodigoPostal { get; set; }
        public string Email { get; set; } = string.Empty;

        // Datos de instructor
        public string Especialidad { get; set; } = string.Empty;
        public int Antiguedad { get; set; }
        public int AdministradorId { get; set; }

        // Datos de profesor
        public TipoClase TipoDeEnsenanza { get; set; }
        public bool Activo { get; set; }

        // Certificación deportiva
        public int? CertificacionId { get; set; }
        public string? NombreCertificacion { get; set; }
        public string? EnteEmisor { get; set; }
        public DateTime? FechaEmision { get; set; }
        public bool? CertificacionVerificada { get; set; }
        public EstadoCertificacion? EstadoCertificacion { get; set; }
    }
}
