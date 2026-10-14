using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.CertificacionesDeportivas.Commands;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.CertificacionesDeportivas.Handlers
{
    public class ResolverCertificacionHandler : IResolverCertificacionHandler
    {
        private readonly ICertificacionDeportivaRepository _repository;

        public ResolverCertificacionHandler(
            ICertificacionDeportivaRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(
            int certificacionId,
            ResolverCertificacionCommand command)
        {
            if (certificacionId <= 0)
                return false;

            if (command.EstadoCertificacion != EstadoCertificacion.Aprobada &&
                command.EstadoCertificacion != EstadoCertificacion.Rechazada)
            {
                return false;
            }

            return await _repository.ResolverAsync(
                certificacionId,
                command.EstadoCertificacion);
        }

    }
}
