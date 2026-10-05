using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Liga : Competencia
    {
        public int Cantidad_fechas { get; set; }

        public int puntos_por_partido { get; set; }
    }
}
