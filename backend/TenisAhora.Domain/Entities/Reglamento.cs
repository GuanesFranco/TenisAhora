using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Reglamento
    {
        public int Id { get; set; }
        public string Nombre { get; set; } 
        public string Contenido { get; set; }
        public bool Vigencia { get; set; }
        public string TipoReglamento { get; set; }

        public bool Activo { get; set; }

        public List<Competencia> Competencias { get; set; } = [];
    }
}
