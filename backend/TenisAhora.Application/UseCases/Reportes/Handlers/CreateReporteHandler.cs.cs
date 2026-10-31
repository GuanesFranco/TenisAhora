using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Reportes.Commands;

namespace TenisAhora.Application.UseCases.Reportes.Handlers
{
    public class CreateReporteHandler : ICreateReporteHandler
    {
        private readonly IReporteRepository _repository;

        public CreateReporteHandler(IReporteRepository repository)
        {
            _repository = repository;
        }

        public async Task<byte[]?> Handle(CreateReporteCommand command)
        {
            return await _repository.GenerarAsync(
                command.TipoReporte,
                command.AdministradorId);
        }


    }
}
