using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Domain.Entities
{
    public class Actividad
    {
        public int Id { get; set; }
        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }
        public int CapacidadMaxma { get; set; }
        public int CantidadDeAlumnos { get; set; }
        public int Duracion { get; set; }

        public int? ProfesorId { get; set; }
        
        public Profesor? Profesor { get; set; }
        public int? EntrenadorId { get; set; }
        public Entrenador? Entrenador { get; set; }
        

        public List<Asistencia> Asistencias { get; set; } = [];

        

        public List<Inscripcion_actividad> InscripcionesActividad { get; set; } = [];

        public bool Activo { get; set; }

        public NivelActividad nivelActividad { get; set; }

    }
}
