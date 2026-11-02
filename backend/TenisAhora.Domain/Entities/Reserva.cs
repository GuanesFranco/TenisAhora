using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Domain.Entities
{
    public class Reserva
    {
        public int Id { get; set; }

        public DateTime fecha_reserva { get; set; }

        public DateTime HoraInicio { get; set; }
        public DateTime HoraFin { get; set; }

        public EstadoReserva estado { get; set; }

        public float importe { get; set; }

        public List<Socio> Socios { get; set; } = [];

        public List<Pago> Pagos { get; set; } = [];


        public List<Detalle_stock_reserva> DetallesStockReserva { get; set; } = [];

        public int CanchaId { get; set; }
        public Cancha Cancha { get; set; }

    }
}
