using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Asistencia
    {
        public int Id { get; private set; }
        private DateTime Fecha { get; set; }
        private bool presente { get; set; }

        public Actividad Actividad { get; set; }

        public Socio Socio { get; set; }

        public Reporte Reporte { get; set; }

    }
}
