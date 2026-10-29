using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Entrenamientos.Queries;

namespace TenisAhora.Application.Interfaces
{
    public interface IGetEntrenamientoByIdHandler
    {
        Task<EntrenamientoResponseDto?> Handle(GetEntrenamientoByIdQuery query);
    }
}