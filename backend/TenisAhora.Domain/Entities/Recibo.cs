using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Recibo
    {
        private int Id { get; set; }

        private DateTime FechaEmision { get; set; }

        private float importe { get; set; }

        public int PagoId { get; set; }
        public Pago Pago { get; set; }

    }
}
