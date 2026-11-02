using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenisAhora.Application.Interfaces;
using TenisAhora.Application.UseCases.CertificacionesDeportivas.Commands;
using TenisAhora.Domain.Entities;
using TenisAhora.Domain.Enums;

namespace TenisAhora.Application.UseCases.CertificacionesDeportivas.Handlers
{
    public class CreateCertificacionDeportivaHandler : ICreateCertificacionDeportivaHandler
    {
        private readonly ICertificacionDeportivaRepository _repository;

        public CreateCertificacionDeportivaHandler(
            ICertificacionDeportivaRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateCertificacionDeportivaCommand command)
        {
            if (!await _repository.InstructorExistsAsync(command.InstructorId))
            {
                throw new KeyNotFoundException(
                    $"No existe un instructor con ID {command.InstructorId}.");
            }

            var certificacion = new CertificacionDeportiva
            {
                NombreCertificacion = command.NombreCertificacion,
                FechaEmision = command.FechaEmision,
                EnteEmisor = command.EnteEmisor,
                InstructorId = command.InstructorId,
                certificacionDeportiva = false,
                EstadoCertificacion = EstadoCertificacion.Pendiente
            };

            return await _repository.CreateAsync(certificacion);
        }


    }
}
