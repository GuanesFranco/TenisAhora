using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Equipo
    {
        private int Id { get; set; }
        private string Nombre { get; set; }

        
        public List<Socio> Socios { get; set; } = [];


        public List<Inscripcion_competencia> InscripcionesCompetencia { get; set; } = [];

    }
}
