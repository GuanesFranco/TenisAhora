using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Profesor : Instructor
    {
        public List<Actividad> Actividades { get; set; } = [];
    }
}
