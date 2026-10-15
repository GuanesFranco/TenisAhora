using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces
{
    public interface IEntrenadorRepository
    {
        Task<Entrenador> CrearAsync(Entrenador entrenador);

       

    }
}
