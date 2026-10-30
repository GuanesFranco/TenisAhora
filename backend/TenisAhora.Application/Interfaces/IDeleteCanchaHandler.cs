using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.UseCases.Canchas.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface IDeleteCanchaHandler
    {

        Task<bool> Handle(DeleteCanchaCommand command);

    }
}
