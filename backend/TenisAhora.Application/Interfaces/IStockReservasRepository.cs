using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces;

public interface IStockReservasRepository
{
    Task<Stock?> ObtenerPorIdAsync(int stockId, CancellationToken cancellationToken = default);
    Task<List<Stock>> ListarTodosAsync(CancellationToken cancellationToken = default);
    Task<bool> ValidarStockDisponibleAsync(int stockId, int cantidad, CancellationToken cancellationToken = default);
    Task DescontarStockAsync(int stockId, int cantidad, CancellationToken cancellationToken = default);
    Task IncrementarStockAsync(int stockId, int cantidad, CancellationToken cancellationToken = default);
}
