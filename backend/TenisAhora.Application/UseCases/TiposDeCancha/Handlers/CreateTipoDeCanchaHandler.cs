using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.TiposDeCancha.Commands;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.UseCases.TiposDeCancha.Handlers
{
    public class CreateTipoDeCanchaHandler : ICreateTipoDeCanchaHandler
    {
        private readonly ITipoDeCanchaRepository _repository;

        public CreateTipoDeCanchaHandler(ITipoDeCanchaRepository repository)
        {
            _repository = repository;
        }

        public async Task<TipoDeCanchaResponseDto> Handle(CreateTipoDeCanchaCommand command)
        {
            var tipoDeCancha = new TipoDeCancha(
                command.Superficie,
                command.Capacidad,
                command.Precio,
                command.Caracteristicas
            );

            var creado = await _repository.CrearAsync(tipoDeCancha);

            return new TipoDeCanchaResponseDto
            {
                Id = creado.Id,
                Superficie = creado.superficie,
                Capacidad = creado.capacidad,
                Precio = creado.precio,
                Caracteristicas = creado.caracteristicas
            };
        }
    }
}

