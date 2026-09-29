using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Domain.Entities
{
    public class TipoDeCancha
    {
        private int Id { get; set; }

        private Superficie superficie { get; set; }

        private int capacidad { get; set; }
        private double precio { get; set; }

        public List<Cancha> Canchas { get; set; } = [];



    }
}
