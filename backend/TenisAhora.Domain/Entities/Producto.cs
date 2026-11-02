using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Producto
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public float Precio { get; set; }

        public int StockId { get; set; }
        public Stock Stock { get; set; }

        public bool Activo { get; set; } = true;
    }
}
