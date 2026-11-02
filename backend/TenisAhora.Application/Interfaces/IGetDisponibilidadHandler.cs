using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Disponibilidad;
using TenisAhora.Application.UseCases.Disponibilidad.Queries;

namespace TenisAhora.Application.Interfaces;

public interface IGetDisponibilidadByCanchaHandler
{
    Task<IEnumerable<DisponibilidadResponseDto>> Handle(GetDisponibilidadByCanchaQuery query, CancellationToken cancellationToken = default);
}

public interface IGetDisponibilidadByIdHandler
{
    Task<DisponibilidadResponseDto?> Handle(GetDisponibilidadByIdQuery query, CancellationToken cancellationToken = default);
}