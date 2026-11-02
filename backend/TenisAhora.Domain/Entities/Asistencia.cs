using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Asistencia
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public bool presente { get; set; }

        public int ActividadId { get; set; }
        public Actividad Actividad { get; set; }

        public int SocioId { get; set; }
        public Socio Socio { get; set; }

       

    }
}
