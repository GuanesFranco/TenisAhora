using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Domain.Entities;

namespace TenisAhora.Application.Interfaces
{
    public interface IProfesorRepository
    {
        Task<Profesor> CrearAsync(Profesor profesor);

        Task<Profesor?> ObtenerPorIdAsync(int id);

        Task<Profesor?> ObtenerPorIdParaActualizarAsync(int id); //busca el profesor para modificarlo.
        Task GuardarCambiosAsync(); // Nuevo: guarda los cambios realizados.
    }
}
