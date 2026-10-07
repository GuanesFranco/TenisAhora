using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Inscripcion_competencia : Inscripcion
    {

        public int NumeroParticipante { get; set; }
        public int CompetenciaId { get; set; }
        public Competencia Competencia { get; set; }

        public List<Equipo> Equipos { get; set; } = [];


    }
}
