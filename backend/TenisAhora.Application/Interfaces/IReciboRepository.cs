using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces;

public interface IReciboRepository
{
    Task<Recibo> AgregarAsync(Recibo recibo, CancellationToken ct = default);
    Task<Recibo?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<Recibo?> ObtenerPorPagoIdAsync(int pagoId, CancellationToken ct = default);
    Task<List<Recibo>> ListarTodosAsync(CancellationToken ct = default);
    Task ActualizarAsync(Recibo recibo, CancellationToken ct = default);
}
