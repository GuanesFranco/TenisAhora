using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Domain.Entities
{
    public class Persona
    {
        private int Id { get; set; }
        private string Nombre { get; set; }
        private string Apellido { get; set; }
        private int Dni { get; set; }
        private DateTime FechaDeNacimiento { get; set; }
        private string Telefono { get; set; }
        private string Direccion { get; set; }
        private int CodigoPostal { get; set; }
        private string Contraseña { get; set; }



    }
}
