using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class CertificacionDeportiva
    {
        public int Id { get; set; }
        public bool certificacionDeportiva { get; set; }
        public DateTime FechaEmision { get; set; }
        public string NombreCertificacion { get; set; }
        public string EnteEmisor { get; set; }

        public int InstructorId { get; set; }
        public Instructor instructor { get; set; }


    }
}
