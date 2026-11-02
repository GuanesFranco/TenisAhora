using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;

namespace TenisAhora.Application.UseCases.Profesores.Handlers
{
    public class DarDeBajaProfesorHandler : IDarDeBajaProfesorHandler
    {
        private readonly IProfesorRepository _profesorRepository;

        public DarDeBajaProfesorHandler(
            IProfesorRepository profesorRepository)
        {
            _profesorRepository = profesorRepository;
        }

        public async Task<bool> Handle(int id)
        {
            if (id <= 0)
                return false;

            return await _profesorRepository.DarDeBajaAsync(id);
        }
    }

}

