using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Inscripcion_actividad : Inscripcion
    {
        public int ActividadId { get; set; }
        public Actividad Actividad { get; set; }

    }
}
