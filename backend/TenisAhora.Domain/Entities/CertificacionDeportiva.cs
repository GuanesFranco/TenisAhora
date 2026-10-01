using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class CertificacionDeportiva
    {
        public int Id { get; private set; }
        private bool certificacionDeportiva { get; set; }
        private DateTime FechaEmision { get; set; }
        private string NombreCertificacion { get; set; }
        private string EnteEmisor { get; set; }

        public Instructor instructor { get; set; }


    }
}
