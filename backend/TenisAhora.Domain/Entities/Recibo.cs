using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Recibo
    {
        public int Id { get; private set; }

        private DateTime FechaEmision { get; set; }

        private float importe { get; set; }

        public Pago Pago { get; set; }

    }
}
