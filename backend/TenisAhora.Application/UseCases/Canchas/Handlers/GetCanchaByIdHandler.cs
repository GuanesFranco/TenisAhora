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
    public class GetCanchaByIdHandler : IGetCanchaByIdHandler
    {
        private readonly ICanchaRepository _repository;

        public GetCanchaByIdHandler(ICanchaRepository repository)
        {
            _repository = repository;
        }

        public async Task<CanchaResponseDto?> Handle(
            GetCanchaByIdQuery query)
        {
            var cancha = await _repository.ObtenerPorIdAsync(query.Id);

            if (cancha == null)
                return null;

            return new CanchaResponseDto
            {
                Id = cancha.Id,
                TipoDeCanchaId = cancha.TipoDeCanchaId,
                Superficie = cancha.TipoDeCancha.superficie.ToString(),
                Capacidad = cancha.TipoDeCancha.capacidad,
                Precio = cancha.TipoDeCancha.precio
            };
        }

    }
}
