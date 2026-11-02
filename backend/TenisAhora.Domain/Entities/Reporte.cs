using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Reporte
    {
        public int Id { get; set; }

        public DateTime FechaGeneracion { get; set; }

        public string tipo_reporte { get; set; }

        public string contenido_detalle { get; set; }

        public int AdministradorId { get; set; }
        public Administrador Administrador { get; set; }

    }
}
