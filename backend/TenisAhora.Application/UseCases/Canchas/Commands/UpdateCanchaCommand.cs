using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Application.UseCases.Canchas.Commands
{
    public class UpdateCanchaCommand
    {
        public int Id { get; set; }
        public int TipoDeCanchaId { get; set; }

    }
}
