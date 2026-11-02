using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;

namespace TenisAhora.Application.UseCases.Entrenadores.Handlers
{
    public class ReactivarEntrenadorHandler : IReactivarEntrenadorHandler
    {
        private readonly IEntrenadorRepository _entrenadorRepository;

        public ReactivarEntrenadorHandler(
            IEntrenadorRepository entrenadorRepository)
        {
            _entrenadorRepository = entrenadorRepository;
        }

        public async Task<bool> Handle(int id)
        {
            return await _entrenadorRepository.ReactivarAsync(id);
        }

    }
}
