using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.TiposDeCancha.Queries;

namespace TenisAhora.Application.Interfaces
{
    public interface IGetTiposDeCanchaHandler
    {
        Task<List<TipoDeCanchaResponseDto>> Handle(GetTiposDeCanchaQuery query);
    }
}

