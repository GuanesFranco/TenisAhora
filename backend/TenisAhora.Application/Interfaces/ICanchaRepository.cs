using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces;

public interface ICanchaRepository
{
    Task<Cancha?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cancha>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
}