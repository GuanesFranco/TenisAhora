using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Reglamento
    {
        private int Id { get; set; }
        private string Nombre { get; set; } 
        private string Contenido { get; set; }
        private bool Vigencia { get; set; }
        private string TipoReglamento { get; set; }

        private List<Competencia> Competencias { get; set; } = [];
    }
}
