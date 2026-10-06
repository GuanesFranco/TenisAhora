using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Domain.Entities
{
    public class Inscripcion_actividad : Inscripcion
    {
        public NivelActividad NivelActividad { get; set; }
        public int ActividadId { get; set; }
        public Actividad Actividad { get; set; }

    }
}
