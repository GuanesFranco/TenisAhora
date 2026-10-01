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
        public int Id { get; private set; }

        private DateTime fecha_reserva { get; set; }

        private DateTime HoraInicio { get; set; }
        private DateTime HoraFin { get; set; }

        private EstadoReserva estado { get; set; }

        private float importe { get; set; }

        public List<Socio> Socios { get; set; } = [];

        public List<Pago> Pagos { get; set; } = [];


        public List<Detalle_stock_reserva> DetallesStockReserva { get; set; } = [];

        public Cancha Cancha { get; set; }

    }
}
