using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Instructor : Persona
    {
        public string especialidad { get; set; }
        public int antiguedad { get; set; }

        public CertificacionDeportiva certificacionDeportiva { get; set; }

        
        public int AdministradorId { get; set; }
        public Administrador Administrador { get; set; }


    }
}
