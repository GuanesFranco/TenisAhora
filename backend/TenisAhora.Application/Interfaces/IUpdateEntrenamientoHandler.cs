using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Entrenamientos.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface IUpdateEntrenamientoHandler
    {
        Task<EntrenamientoResponseDto?> Handle(int id, UpdateEntrenamientoCommand command);
    }
}