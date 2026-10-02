using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Reporte
    {
        private int Id { get; set; }

        private DateTime FechaGeneracion { get; set; }

        private string tipo_reporte { get; set; }

        private string contenido_detalle { get; set; }

        public Administrador Administrador { get; set; }


        public List<Asistencia> Asistencias { get; set; } = [];

       


    }
}
