using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Entrenadores.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface IUpdateEntrenadorHandler
    {
        Task<EntrenadorResponseDto?> Handle( int id, UpdateEntrenadorCommand command);

    }
}
