using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Asistencia
    {
        private int Id { get; set; }
        private DateTime Fecha { get; set; }
        private bool presente { get; set; }

        public int ActividadId { get; set; }
        public Actividad Actividad { get; set; }

        public int SocioId { get; set; }
        public Socio Socio { get; set; }

       

    }
}
