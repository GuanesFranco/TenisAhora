using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Canchas;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Canchas.Commands;

namespace TenisAhora.Application.UseCases.Canchas.Handlers
{
    public class UpdateCanchaHandler : IUpdateCanchaHandler
    {
        private readonly ICanchaRepository _repository;

        public UpdateCanchaHandler(ICanchaRepository repository)
        {
            _repository = repository;
        }

        public async Task<CanchaResponseDto?> Handle(
            UpdateCanchaCommand command)
        {
            var cancha = await _repository.ActualizarAsync(
                command.Id,
                command.TipoDeCanchaId,
                command.FotoUrl);

            if (cancha == null)
                return null;

            return new CanchaResponseDto
            {
                Id = cancha.Id,
                TipoDeCanchaId = cancha.TipoDeCanchaId,
                Superficie = cancha.TipoDeCancha.superficie.ToString(),
                Capacidad = cancha.TipoDeCancha.capacidad,
                Precio = cancha.TipoDeCancha.precio,
                FotoUrl = cancha.FotoUrl
            };
        }
    }
}
