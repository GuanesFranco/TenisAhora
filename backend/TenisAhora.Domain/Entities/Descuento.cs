using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Descuento
    {
        private int Id { get; set; }

        private float porcentaje { get; set; }
         private string descripcion { get; set; }

        private string condiciones { get; set; }

        public List<Pago> Pagos { get; set; } = [];

        public int AdministradorId { get; set; }
        public Administrador Administrador { get; set; }


    }
}
