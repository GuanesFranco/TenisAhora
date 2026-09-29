using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Disponibilidad
    {
        private int Id { get; set; }

        private DateTime FechaHoraInicio { get; set; }

        private DateTime FechaHoraFin { get; set; }

        public Cancha Cancha { get; set; }



    }
}
