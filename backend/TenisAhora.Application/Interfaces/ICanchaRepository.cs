using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces
{
    public interface ICanchaRepository
    {
        Task<Cancha?> CrearAsync(int tipoDeCanchaId);

        Task<bool> EliminarAsync(int id);
        Task<Cancha?> ObtenerPorIdAsync(int id);
        Task<List<Cancha>> ObtenerTodasAsync();
        Task<Cancha?> ActualizarAsync(int id, int tipoDeCanchaId);

    }
}
