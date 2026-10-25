using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.TiposDeCancha.Queries;

namespace TenisAhora.Application.UseCases.TiposDeCancha.Handlers
{
    public class GetTipoDeCanchaByIdHandler : IGetTipoDeCanchaByIdHandler
    {
        private readonly ITipoDeCanchaRepository _repository;

        public GetTipoDeCanchaByIdHandler(ITipoDeCanchaRepository repository)
        {
            _repository = repository;
        }

        public async Task<TipoDeCanchaResponseDto?> Handle(GetTipoDeCanchaByIdQuery query)
        {
            var tipoDeCancha = await _repository.ObtenerPorIdAsync(query.Id);

            if (tipoDeCancha is null)
            {
                return null;
            }

            return new TipoDeCanchaResponseDto
            {
                Id = tipoDeCancha.Id,
                Superficie = tipoDeCancha.superficie,
                Capacidad = tipoDeCancha.capacidad,
                Precio = tipoDeCancha.precio,
                Caracteristicas = tipoDeCancha.caracteristicas
            };
        }
    }
}

