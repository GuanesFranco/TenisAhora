using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Cancha
    {
        public int Id { get; private set; }

        public List<Reserva> Reservas { get; set; } = [];

        public List<Disponibilidad> Disponibilidades { get; set; } = [];
        public List<Partido> Partidos { get; set; } = [];

        public TipoDeCancha TipoDeCancha { get; set; }

    }
}
