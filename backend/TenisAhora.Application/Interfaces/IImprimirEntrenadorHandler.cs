using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;

namespace TenisAhora.Application.Interfaces
{
    public interface IImprimirEntrenadorHandler
    {
        Task<EntrenadorResponseDto?> Handle(int id);

    }
}
