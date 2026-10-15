using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.Profesores.Commands
{
    public class UpdateProfesorCommand
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public int Dni { get; set; }
        public DateTime FechaDeNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int CodigoPostal { get; set; }
        public string Email { get; set; } = string.Empty;

        // Datos del instructor
        public string Especialidad { get; set; } = string.Empty;
        public int Antiguedad { get; set; }

        // Datos específicos del profesor
        public TipoClase TipoDeEnsenanza { get; set; }


    }
}
