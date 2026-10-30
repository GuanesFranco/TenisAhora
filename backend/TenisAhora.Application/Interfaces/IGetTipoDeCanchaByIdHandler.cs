using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.TiposDeCancha.Queries;

namespace TenisAhora.Application.Interfaces
{
    public interface IGetTipoDeCanchaByIdHandler
    {
        Task<TipoDeCanchaResponseDto?> Handle(GetTipoDeCanchaByIdQuery query);
    }
}

