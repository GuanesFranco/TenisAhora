using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Clases.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface IUpdateClaseHandler
    {
        Task<ClaseResponseDto?> Handle(int id, UpdateClaseCommand command);
    }
}