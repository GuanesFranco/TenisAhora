using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Actividad
    {
        private DateTime HoraInicio { get; set; }
        private DateTime HoraFin { get; set; }
        private int CapacidadMaxma { get; set; }
        private int CantidadDeAlumnos { get; set; }
        private int Duracion { get; set; }

        public Profesor? Profesor { get; set; }
        public Entrenador? Entrenador { get; set; }

        public List<Asistencia> Asistencias { get; set; } = [];

        public Reporte Reporte { get; set; }

        public List<Inscripcion_actividad> InscripcionesActividad { get; set; } = [];

    }
}
