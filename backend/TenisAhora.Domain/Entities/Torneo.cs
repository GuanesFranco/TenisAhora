using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Torneo : Competencia
    {
        public string etapa_actual { get; set; }
        public string tipo_llave { get; set; }


    }
}
