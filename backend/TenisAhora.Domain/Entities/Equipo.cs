using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Equipo
    {
        public int Id { get; private set; }
        private string Nombre { get; set; }

        
        public List<Socio> Socios { get; set; } = [];


        public List<Inscripcion_competencia> InscripcionesCompetencia { get; set; } = [];

    }
}
