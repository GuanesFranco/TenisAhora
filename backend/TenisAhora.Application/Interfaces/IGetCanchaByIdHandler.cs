using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Canchas;
using TenisAhora.Application.UseCases.Canchas.Queries;

namespace TenisAhora.Application.Interfaces
{
    public interface IGetCanchaByIdHandler
    {
        Task<CanchaResponseDto?> Handle(GetCanchaByIdQuery query);
    }
}
