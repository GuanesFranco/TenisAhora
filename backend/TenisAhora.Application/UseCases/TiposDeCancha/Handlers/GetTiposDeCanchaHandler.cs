using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.TiposDeCancha.Queries;

namespace TenisAhora.Application.UseCases.TiposDeCancha.Handlers
{
    public class GetTiposDeCanchaHandler : IGetTiposDeCanchaHandler
    {
        private readonly ITipoDeCanchaRepository _repository;

        public GetTiposDeCanchaHandler(ITipoDeCanchaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<TipoDeCanchaResponseDto>> Handle(GetTiposDeCanchaQuery query)
        {
            var lista = await _repository.ObtenerTodosAsync();

            return lista.Select(t => new TipoDeCanchaResponseDto
            {
                Id = t.Id,
                Superficie = t.superficie,
                Capacidad = t.capacidad,
                Precio = t.precio,
                Caracteristicas = t.caracteristicas
            }).ToList();
        }
    }
}

