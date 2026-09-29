using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Partido
    {
        private int Id { get; set; }
        private DateTime fecha { get; set; }
        private DateTime HoraInicio { get; set; }

        private DateTime HoraFin { get; set; }
         private string ronda { get; set; }
        private string resultado { get; set; }

        public Competencia Competencia { get; set; }

        public Cancha Cancha { get; set; }

    }
}
