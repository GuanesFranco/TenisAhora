using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Profesores.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface ICreateProfesorHandler
    {

        Task<ProfesorResponseDto> Handle(CreateProfesorCommand command);
    }
}
