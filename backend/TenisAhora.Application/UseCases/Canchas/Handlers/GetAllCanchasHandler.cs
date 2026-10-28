using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Canchas;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Canchas.Queries;

namespace TenisAhora.Application.UseCases.Canchas.Handlers
{
    public class GetAllCanchasHandler : IGetAllCanchasHandler
    {
        private readonly ICanchaRepository _repository;

        public GetAllCanchasHandler(ICanchaRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CanchaResponseDto>> Handle(
            GetAllCanchasQuery query)
        {
            var canchas = await _repository.ObtenerTodasAsync();

            return canchas.Select(c => new CanchaResponseDto
            {
                Id = c.Id,
                TipoDeCanchaId = c.TipoDeCanchaId,
                Superficie = c.TipoDeCancha.superficie.ToString(),
                Capacidad = c.TipoDeCancha.capacidad,
                Precio = c.TipoDeCancha.precio
            }).ToList();
        }



    }
}
