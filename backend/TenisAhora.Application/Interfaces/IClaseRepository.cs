using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces
{
    public interface IClaseRepository
    {
        Task<Clase> CrearAsync(Clase clase);
        Task<Clase?> ObtenerPorIdAsync(int id);
        Task<IEnumerable<Clase>> ObtenerTodasAsync();
        Task ModificarAsync(Clase clase);
        Task EliminarAsync(int id);
    }
}