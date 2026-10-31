using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces;

public interface IDisponibilidadRepository
{
    Task<Disponibilidad?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Disponibilidad>> ObtenerPorCanchaAsync(int canchaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Disponibilidad>> ObtenerPorCanchaYFiltrosAsync(int canchaId, DateTime? fechaDesde, DateTime? fechaHasta, TenisAhora.Domain.Enums.EstadoDisponibilidad? estado, CancellationToken cancellationToken = default);
    Task<bool> ExisteSolapamientoAsync(int canchaId, DateTime inicio, DateTime fin, CancellationToken cancellationToken = default);
    Task<bool> ExisteSolapamientoAsync(int canchaId, DateTime inicio, DateTime fin, int excluirDisponibilidadId, CancellationToken cancellationToken = default);
    Task CrearAsync(Disponibilidad disponibilidad, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Disponibilidad disponibilidad, CancellationToken cancellationToken = default);
    Task<bool> EliminarAsync(int id, CancellationToken cancellationToken = default);
}