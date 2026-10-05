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
        public int Id { get; set; }

        public Superficie superficie { get; set; }

        public int capacidad { get; set; }
        public double precio { get; set; }

        public List<Cancha> Canchas { get; set; } = [];



    }
}
