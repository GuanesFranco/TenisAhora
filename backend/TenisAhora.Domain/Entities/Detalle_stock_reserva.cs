using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Detalle_stock_reserva
    {
        public int Id { get; set; }

        public int cantidad { get; set; }

        public int ReservaId { get; set; }
        public Reserva Reserva { get; set; }

        public int StockId { get; set; }
        public Stock Stock { get; set; }
    }
}
