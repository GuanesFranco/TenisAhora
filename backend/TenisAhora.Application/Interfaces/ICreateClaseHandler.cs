using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Clases.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface ICreateClaseHandler
    {
        Task<ClaseResponseDto> Handle(CreateClaseCommand command);
    }
}