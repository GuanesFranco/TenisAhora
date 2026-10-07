using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Domain.Entities
{
    public class Disponibilidad
    {
        public int Id { get; set; }

        public DateTime FechaHoraInicio { get; set; }

        public DateTime FechaHoraFin { get; set; }

        public EstadoDisponibilidad estado { get; set; }

        public int CanchaId { get; set; }
        public Cancha Cancha { get; set; }



    }
}
