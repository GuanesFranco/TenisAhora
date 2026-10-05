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
        public int Id { get; set; }

        public float importe { get; set; }
        public DateTime Fecha_pago { get; set; }
         public TipoDePago tipo_pago { get; set; }

        public EstadoPago estado { get; set; }

        public float porcentaje_pago { get; set; }
        public int? ReservaId { get; set; }
        public Reserva? Reserva { get; set; }

        public int? InscripcionId { get; set; }
        public Inscripcion? Inscripcion { get; set; }

        public Recibo Recibo { get; set; }

        public int? DescuentoId { get; set; }
        public Descuento? Descuento { get; set; }


    }
}
