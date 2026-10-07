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
        public int Id { get; set; }

        public string Nombre  { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public CategoriaGenero categoriaGenero { get; set; }

        public Modalidad modalidad { get; set; }

        public EstadoCompetencia estado { get; set; }

        public int ReglamentoId { get; set; }
        public Reglamento Reglamento { get; set; }

        public List<Inscripcion_competencia> Inscripciones { get; set; } = [];

        public List<Partido> Partidos { get; set; } = [];

        public bool Activo { get; set; }

    }
}
