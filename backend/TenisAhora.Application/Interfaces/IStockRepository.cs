using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces;

public interface IStockRepository
{
    Task<Stock?> ObtenerPorIdAsync(int stockId, CancellationToken cancellationToken = default);
    Task<List<Stock>> ListarTodosAsync(CancellationToken cancellationToken = default);
    Task<bool> ValidarStockDisponibleAsync(int stockId, int cantidad, CancellationToken cancellationToken = default);
    Task DescontarStockAsync(int stockId, int cantidad, CancellationToken cancellationToken = default);
    Task IncrementarStockAsync(int stockId, int cantidad, CancellationToken cancellationToken = default);
    Task<Stock> AgregarAsync(Stock stock, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Stock stock, CancellationToken cancellationToken = default);
    Task EliminarAsync(Stock stock, CancellationToken cancellationToken = default);
    Task<bool> TieneReservasAsociadasAsync(int stockId, CancellationToken cancellationToken = default);
}
