using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Domain.Entities
{
    public class Socio : Persona
    {
        public EstadoMembresia estadoMembresia { get; set; }

        public List<Asistencia> Asistencias { get; set; } = [];

        public List<Equipo> Equipos { get; set; } = [];

        public List<Inscripcion> Inscripciones { get; set; } = [];

        // Socio
        public List<Reserva> Reservas { get; set; } = [];

    }
}
