using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Recibo
    {
        public int Id { get; set; }

        public DateTime FechaEmision { get; set; }

        public float importe { get; set; }

        public int PagoId { get; set; }
        public Pago Pago { get; set; }

    }
}
