using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;

namespace TenisAhora.Application.UseCases.Entrenamientos.Handlers
{
    public class DarDeBajaEntrenamientoHandler : IDarDeBajaEntrenamientoHandler
    {
        private readonly IEntrenamientoRepository _entrenamientoRepository;

        public DarDeBajaEntrenamientoHandler(IEntrenamientoRepository entrenamientoRepository)
        {
            _entrenamientoRepository = entrenamientoRepository;
        }

        public async Task<bool> Handle(int id)
        {
            var e = await _entrenamientoRepository.ObtenerPorIdAsync(id);
            if (e == null) return false;

            await _entrenamientoRepository.EliminarAsync(id);
            return true;
        }
    }
}