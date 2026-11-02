using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Entrenamientos.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface ICreateEntrenamientoHandler
    {
        Task<EntrenamientoResponseDto> Handle(CreateEntrenamientoCommand command);
    }
}