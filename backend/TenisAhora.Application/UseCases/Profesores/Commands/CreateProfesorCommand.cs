using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Profesores.Commands
{
    public class CreateProfesorCommand
    {
        // Datos personales
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public int Dni { get; set; }
        public DateTime FechaDeNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int CodigoPostal { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Datos de instructor
        public string Especialidad { get; set; } = string.Empty;
        public int Antiguedad { get; set; }
        public int AdministradorId { get; set; }

        // Datos específicos del profesor
        public TipoClase TipoDeEnsenanza { get; set; }

        // Datos de la certificación deportiva
        public CertificacionDeportivaCommand Certificacion { get; set; } = new();


    }


    public class CertificacionDeportivaCommand
    {
        public DateTime FechaEmision { get; set; }
        public string NombreCertificacion { get; set; } = string.Empty;
        public string EnteEmisor { get; set; } = string.Empty;
    }
}
