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
        private int Id { get; set; }

        private DateTime FechaHoraInicio { get; set; }

        private DateTime FechaHoraFin { get; set; }

        private EstadoDisponibilidad estado { get; set; }

        public Cancha Cancha { get; set; }



    }
}
