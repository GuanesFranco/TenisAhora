using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Domain.Entities
{
    public class Entrenamiento : Actividad
    {
        public string Nombre { get; set; } = string.Empty;
        public ObjetivoEntrenamiento objetivoEntrenamiento { get; set; }
    }
}