using System.Collections.Generic;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.Interfaces
{
    public interface ITipoDeCanchaRepository
    {
        Task<TipoDeCancha> CrearAsync(TipoDeCancha tipoDeCancha);
        Task<TipoDeCancha?> ObtenerPorIdAsync(int id);
        Task<TipoDeCancha?> ObtenerPorIdParaActualizarAsync(int id);
        Task<List<TipoDeCancha>> ObtenerTodosAsync();
        Task ActualizarAsync(TipoDeCancha tipoDeCancha);
        Task<bool> TieneCanchasAsociadasAsync(int id);
        Task<bool> EliminarAsync(int id);
        Task GuardarCambiosAsync();
    }
}

