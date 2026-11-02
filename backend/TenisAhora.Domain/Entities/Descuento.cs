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
        public int Id { get; set; }

        public float porcentaje { get; set; }
         public string descripcion { get; set; }

        public string condiciones { get; set; }

        public List<Pago> Pagos { get; set; } = [];

        public int AdministradorId { get; set; }
        public Administrador Administrador { get; set; }


    }
}
