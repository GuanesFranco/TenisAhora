using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Entrenamientos.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface IRegistrarAsistenciaEntrenamientoHandler
    {
        Task<AsistenciaEntrenamientoResponseDto> Handle(RegistrarAsistenciaEntrenamientoCommand command);
    }
}