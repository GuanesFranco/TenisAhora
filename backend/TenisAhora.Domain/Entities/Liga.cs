using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Liga : Competencia
    {
        private int Cantidad_fechas { get; set; }

        private int puntos_por_partido { get; set; }
    }
}
