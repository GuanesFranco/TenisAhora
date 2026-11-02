using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;

namespace TenisAhora.Application.UseCases.Clases.Handlers
{
    public class DarDeBajaClaseHandler : IDarDeBajaClaseHandler
    {
        private readonly IClaseRepository _claseRepository;

        public DarDeBajaClaseHandler(IClaseRepository claseRepository)
        {
            _claseRepository = claseRepository;
        }

        public async Task<bool> Handle(int id)
        {
            var clase = await _claseRepository.ObtenerPorIdAsync(id);
            if (clase == null) return false;

            await _claseRepository.EliminarAsync(id);
            return true;
        }
    }
}