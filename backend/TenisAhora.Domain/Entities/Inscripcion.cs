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
        public int Id { get; set; }
        public DateTime Fecha { get; set; }

        public EstadoInscripcion estadoInscripcion { get; set; }

        public int SocioId { get; set; }
        public Socio Socio { get; set; }

        public List<Pago> Pagos { get; set; } = [];

    }
}
