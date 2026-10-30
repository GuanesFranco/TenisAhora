using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.TiposDeCancha.Commands;

namespace TenisAhora.Application.UseCases.TiposDeCancha.Handlers
{
    public class UpdateTipoDeCanchaHandler : IUpdateTipoDeCanchaHandler
    {
        private readonly ITipoDeCanchaRepository _repository;

        public UpdateTipoDeCanchaHandler(ITipoDeCanchaRepository repository)
        {
            _repository = repository;
        }

        public async Task<TipoDeCanchaResponseDto?> Handle(int id, UpdateTipoDeCanchaCommand command)
        {
            var tipoDeCancha = await _repository.ObtenerPorIdParaActualizarAsync(id);

            if (tipoDeCancha is null)
            {
                return null;
            }

            tipoDeCancha.Actualizar(
                command.Superficie,
                command.Capacidad,
                command.Precio,
                command.Caracteristicas
            );

            await _repository.GuardarCambiosAsync();

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

