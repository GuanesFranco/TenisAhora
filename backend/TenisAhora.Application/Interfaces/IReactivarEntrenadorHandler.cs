using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenisAhora.Application.Interfaces
{
    public interface IReactivarEntrenadorHandler
    {
        Task<bool> Handle(int id);

    }
}
