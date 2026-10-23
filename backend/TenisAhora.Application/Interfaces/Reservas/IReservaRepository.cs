using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces;

public interface IReservaRepository
{
    Task<Reserva?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reserva>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
    Task<bool> ExisteSuperposicionAsync(int canchaId, DateTime inicio, DateTime fin, CancellationToken cancellationToken = default);
    Task<bool> ExisteSuperposicionAsync(int canchaId, DateTime inicio, DateTime fin, int? excluirReservaId, CancellationToken cancellationToken = default);
    Task AgregarReservaAsync(Reserva reserva, CancellationToken cancellationToken = default);
    Task ActualizarReservaAsync(Reserva reserva, CancellationToken cancellationToken = default);
}