using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.DTOs.Canchas;
using TenisAhora.Application.UseCases.Canchas.Commands;

namespace TenisAhora.Application.Interfaces
{
    public interface ICreateCanchaHandler
    {
        Task<CanchaResponseDto?> Handle(CreateCanchaCommand command);


    }
}
