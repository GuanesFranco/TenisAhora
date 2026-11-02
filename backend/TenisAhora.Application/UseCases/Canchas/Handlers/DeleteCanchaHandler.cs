using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.Canchas.Commands;

namespace TenisAhora.Application.UseCases.Canchas.Handlers
{
    public class DeleteCanchaHandler : IDeleteCanchaHandler
    {

        private readonly ICanchaRepository _repository;

        public DeleteCanchaHandler(ICanchaRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteCanchaCommand command)
        {
            return await _repository.EliminarAsync(command.Id);
        }



    }
}
