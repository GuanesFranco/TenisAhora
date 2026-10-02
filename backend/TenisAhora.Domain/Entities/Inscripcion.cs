using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Domain.Entities
{
    public class Inscripcion
    {
        private int Id { get; set; }
        private DateTime Fecha { get; set; }

        private EstadoInscripcion estadoInscripcion { get; set; }

        public int SocioId { get; set; }
        public Socio Socio { get; set; }

        public List<Pago> Pagos { get; set; } = [];

    }
}
