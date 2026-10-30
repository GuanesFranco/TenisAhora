using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;

namespace TenisAhora.Application.UseCases.Profesores.Handlers
{
    public class ReactivarProfesorHandler : IReactivarProfesorHandler
    {
        private readonly IProfesorRepository _profesorRepository;

        public ReactivarProfesorHandler(
            IProfesorRepository profesorRepository)
        {
            _profesorRepository = profesorRepository;
        }

        public async Task<bool> Handle(int id)
        {
            return await _profesorRepository.ReactivarAsync(id);
        }

    }
}
