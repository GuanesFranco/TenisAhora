using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Clases.Queries;

namespace TenisAhora.Application.Interfaces
{
    public interface IGetClaseByIdHandler
    {
        Task<ClaseResponseDto?> Handle(GetClaseByIdQuery query);
    }
}