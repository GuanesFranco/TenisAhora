using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.TiposDeCancha.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface ICreateTipoDeCanchaHandler
    {
        Task<TipoDeCanchaResponseDto> Handle(CreateTipoDeCanchaCommand command);
    }
}

