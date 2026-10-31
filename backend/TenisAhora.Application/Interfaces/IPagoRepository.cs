using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces;

public interface IPagoRepository
{
    Task<Pago> AgregarAsync(Pago pago, CancellationToken ct = default);
    Task<Pago?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<List<Pago>> ListarTodosAsync(CancellationToken ct = default);
    Task<List<Pago>> ObtenerPorReservaIdAsync(int reservaId, CancellationToken ct = default);
    Task ActualizarAsync(Pago pago, CancellationToken ct = default);
    Task<Descuento?> ObtenerDescuentoPorIdAsync(int descuentoId, CancellationToken ct = default);
}
