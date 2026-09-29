using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Stock
    {
        private int Id { get; set; }

        private string tipo_elemento { get; set; }

        private int cantidad_disponible { get; set; }

        public List<Detalle_stock_reserva> DetallesStockReserva { get; set; } = [];


    }
}
