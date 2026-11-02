using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Application.UseCases.Reportes.Commands
{
    public class CreateReporteCommand
    {
        public string TipoReporte { get; set; } = string.Empty;

        public int AdministradorId { get; set; }

    }
}
