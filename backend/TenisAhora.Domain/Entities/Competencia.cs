using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Domain.Entities
{
    public class Competencia
    {
        private int Id { get; set; }

        private string Nombre  { get; set; }

        private DateTime FechaInicio { get; set; }

        private DateTime FechaFin { get; set; }

        private CategoriaGenero categoriaGenero { get; set; }

        private Modalidad modalidad { get; set; }

        private EstadoCompetencia estado { get; set; }

        private Reglamento reglamento { get; set; }






        public List<Inscripcion_competencia> Inscripciones { get; set; } = [];

        public List<Partido> Partidos { get; set; } = [];

    }
}
