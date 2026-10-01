using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Domain.Entities
{
    public class Pago
    {
        public int Id { get; private set; }

        private float importe { get; set; }
        private DateTime Fecha_pago { get; set; }
        private TipoDePago tipo_pago { get; set; }

        private EstadoPago estado { get; set; }

        private float porcentaje_pago { get; set; }
        public Reserva Reserva { get; set; }

        public Inscripcion Inscripcion { get; set; }

        public Recibo Recibo { get; set; }

        public List<Descuento> Descuentos { get; set; } = [];


    }
}
