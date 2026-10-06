using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Domain.Entities
{
    public class Entrenador : Instructor
    {
        public ObjetivoEntrenamiento Objetivo { get; set; }
        public List<Actividad> Actividades { get; set; } = [];

    }
}
