using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Partido
    {
        public int Id { get; set; }
        public DateTime fecha { get; set; }
        public DateTime HoraInicio { get; set; }

        public DateTime HoraFin { get; set; }
         public string ronda { get; set; }
        public string resultado { get; set; }

        public int CompetenciaId { get; set; }
        public Competencia Competencia { get; set; }

        public int CanchaId { get; set; }
        public Cancha Cancha { get; set; }

    }
}
