using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Profesores.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface IUpdateProfesorHandler
    {
        Task<ProfesorResponseDto?> Handle(int id, UpdateProfesorCommand command);



    }
}
