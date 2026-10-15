using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Application.UseCases.CertificacionesDeportivas.Commands
{
    public class CreateCertificacionDeportivaCommand
    {
        public string NombreCertificacion { get; set; }
        public DateTime FechaEmision { get; set; }
        public string EnteEmisor { get; set; }
        public int InstructorId { get; set; }




    }
}
