using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Clases.Queries;

namespace TenisAhora.Application.Interfaces
{
    public interface IGetClasesHandler
    {
        Task<IEnumerable<ClaseResponseDto>> Handle(GetClasesQuery query);
    }
}