using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Clases.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface IRegistrarAsistenciaClaseHandler
    {
        Task<AsistenciaClaseResponseDto> Handle(RegistrarAsistenciaClaseCommand command);
    }
}