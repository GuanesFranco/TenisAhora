using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Entrenamientos.Queries;

namespace TenisAhora.Application.Interfaces
{
    public interface IGetEntrenamientosHandler
    {
        Task<IEnumerable<EntrenamientoResponseDto>> Handle(GetEntrenamientosQuery query);
    }
}