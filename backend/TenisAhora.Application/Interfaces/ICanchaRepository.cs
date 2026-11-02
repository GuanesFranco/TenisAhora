using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces;

public interface ICanchaRepository
{
    Task<Cancha?> CrearAsync(int tipoDeCanchaId, string? fotoUrl = null);
    Task<bool> EliminarAsync(int id);
    Task<Cancha?> ObtenerPorIdAsync(int id);
    Task<List<Cancha>> ObtenerTodasAsync();
    Task<Cancha?> ActualizarAsync(int id, int tipoDeCanchaId, string? fotoUrl = null);
}
