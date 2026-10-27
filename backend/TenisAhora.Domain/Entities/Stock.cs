using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Stock
    {
        public int Id { get; set; }

        public string tipo_elemento { get; set; }

        public int cantidad_disponible { get; set; }

        public List<Detalle_stock_reserva> DetallesStockReserva { get; set; } = [];

        // Relación 1 a N: 1 Stock puede tener muchos Productos
        public List<Producto> Productos { get; set; } = [];
    }
}
