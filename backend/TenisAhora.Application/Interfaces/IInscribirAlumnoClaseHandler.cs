using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Clases.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface IInscribirAlumnoClaseHandler
    {
        Task<InscripcionClaseResponseDto> Handle(InscribirAlumnoClaseCommand command);
    }
}