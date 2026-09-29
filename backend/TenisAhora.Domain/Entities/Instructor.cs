using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Instructor : Persona
    {
        private string especialidad { get; set; }
        private int antiguedad { get; set; }

        public CertificacionDeportiva certificacionDeportiva;


    }
}
