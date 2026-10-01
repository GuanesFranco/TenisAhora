using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Reglamento
    {
        public int Id { get; private set; }
        private string Nombre { get; set; } 
        private string Contenido { get; set; }
        private bool Vigencia { get; set; }
        private string TipoReglamento { get; set; }

        public List<Competencia> Competencias { get; set; } = [];
    }
}
