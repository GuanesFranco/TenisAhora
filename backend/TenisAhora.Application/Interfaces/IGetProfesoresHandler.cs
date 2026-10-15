using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs;
using TenisAhora.Application.UseCases.Profesores.Queries;

namespace TenisAhora.Application.Interfaces
{
    public interface IGetProfesoresHandler
    {
        Task<List<ProfesorResponseDto>> Handle(GetProfesoresQuery query);

    }
}
