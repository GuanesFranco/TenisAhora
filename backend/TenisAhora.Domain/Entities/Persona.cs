using System;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Domain.Entities
{
    public class Persona
    {
        public int Id { get; set; }
        public string Nombre { get; set; } 
        public string Apellido { get; set; } 
        public int Dni { get; set; }
        public DateTime FechaDeNacimiento { get; set; }
        public string Telefono { get; set; } 
        public string Direccion { get; set; } 
        public int CodigoPostal { get; set; }
        public string Email { get;  set; } 
        public string PasswordHash { get;  set; } 
        public Rol Rol { get; set; }
    }
}
