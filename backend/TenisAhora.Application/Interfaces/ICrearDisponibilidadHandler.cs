using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Disponibilidad;
using TenisAhora.Application.UseCases.Disponibilidad.Commands;

namespace TenisAhora.Application.Interfaces;

public interface ICrearDisponibilidadHandler
{
    Task<DisponibilidadResponseDto> Handle(CrearDisponibilidadCommand command, CancellationToken cancellationToken = default);
}