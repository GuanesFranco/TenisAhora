using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Domain.Exceptions;

namespace TenisAhora.Application.UseCases.TiposDeCancha.Handlers
{
    public class DeleteTipoDeCanchaHandler : IDeleteTipoDeCanchaHandler
    {
        private readonly ITipoDeCanchaRepository _repository;

        public DeleteTipoDeCanchaHandler(ITipoDeCanchaRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(int id)
        {
            var tipoDeCancha = await _repository.ObtenerPorIdAsync(id);

            if (tipoDeCancha is null)
            {
                return false;
            }

            if (await _repository.TieneCanchasAsociadasAsync(id))
            {
                throw new DomainException("No se puede eliminar el tipo de cancha porque tiene canchas asociadas.");
            }

            return await _repository.EliminarAsync(id);
        }
    }
}

