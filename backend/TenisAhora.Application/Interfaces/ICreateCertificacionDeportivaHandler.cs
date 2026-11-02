using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.UseCases.CertificacionesDeportivas.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface ICreateCertificacionDeportivaHandler
    {
        Task<int> Handle(CreateCertificacionDeportivaCommand command);
    }
}
