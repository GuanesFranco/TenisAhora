using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces
{
    public interface IEntrenadorRepository
    {
        Task<Entrenador> CrearAsync(Entrenador entrenador);

        Task<List<Entrenador>> ObtenerTodosAsync();

        Task<bool> DarDeBajaAsync(int id);

        //para modificar un entrenador
        Task<Entrenador?> ObtenerPorIdParaActualizarAsync(int id);

        Task GuardarCambiosAsync(); //


        Task<Entrenador?> ObtenerPorIdAsync(int id);

        Task<Entrenador?> ObtenerParaImprimirAsync(int id);

        Task<bool> ReactivarAsync(int id);


    }
}
