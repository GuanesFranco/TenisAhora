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
        public int Id { get; private set; }

        private float porcentaje { get; set; }
        private string descripcion { get; set; }

        private string condiciones { get; set; }

        public Pago? Pago { get; set; }




    }
}
