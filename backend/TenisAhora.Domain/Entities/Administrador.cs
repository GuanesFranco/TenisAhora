using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Administrador : Persona
    {
        
        public List<Reporte> Reportes { get; set; } = [];

       
        public List<Instructor> Instructores { get; set; } = [];

        public List<Descuento> Descuentos { get; set; } = [];
    }
}
